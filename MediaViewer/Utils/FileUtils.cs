using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaViewer.GenericEventArgs;
using MediaViewer.Progress;

namespace MediaViewer.Utils
{
    class FileUtils
    {

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private enum CopyFileCallbackAction
        {
            CONTINUE = 0,
            CANCEL = 1,
            STOP = 2,
            QUIET = 3
        }

        private enum CopyFileOptions
        {
            NONE = 0x0,
            FAIL_IF_DESTINATION_EXISTS = 0x1,
            RESTARTABLE = 0x2,
            ALLOW_DECRYPTED_DESTINATION = 0x8,
            ALL = FAIL_IF_DESTINATION_EXISTS | RESTARTABLE | ALLOW_DECRYPTED_DESTINATION
        }

        private enum CopyProgressCallbackReason
        {
            CALLBACK_CHUNK_FINISHED = 0x0,
            CALLBACK_STREAM_SWITCH = 0x1
        }

        private enum CreateFileAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        private enum CreateFileShare
        {
            NONE = 0x00000000,
            FILE_SHARE_READ = 0x00000001
        }

        private enum CreateFileCreationDisposition
        {
            OPEN_EXISTING = 3
        }

        private enum CreateFileAttributes
        {
            NORMAL = 0x00000080,
            FILE_FLAG_RANDOM_ACCESS = 0x10000000
        }

        private const int ERROR_SHARING_VIOLATION = 32;

        private enum Action
        {
            COPY,
            MOVE
        }

        delegate int CopyProgressDelegate(
            long totalFileSize, long totalBytesTransferred, long streamSize,
            long streamBytesTransferred, int streamNumber, CopyProgressCallbackReason callbackReason,
            IntPtr sourceFile, IntPtr destinationFile, IntPtr data);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern bool CopyFileEx(
            string lpExistingFileName, string lpgcnewFileName,
            CopyProgressDelegate lpProgressRoutine,
            IntPtr lpData, ref int pbCancel, int dwCopyFlags);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        static extern IntPtr CreateFileW(
           string lpFileName,
           uint dwDesiredAccess,
           int dwShareMode,
           IntPtr lpSecurityAttributes,
           int dwCreationDisposition,
           int dwFlagsAndAttributes,
           IntPtr hTemplateFile);

        int copyProgress(long totalFileSize, long totalBytesTransferred, long streamSize,
            long streamBytesTransferred, int streamNumber, CopyProgressCallbackReason callbackReason,
            IntPtr sourceFile, IntPtr destinationFile, IntPtr data)
        {

            int fileProgress = (int)((100 * totalBytesTransferred) / totalFileSize);

            GCHandle h = GCHandle.FromIntPtr(data);
            IProgress progress = (IProgress)h.Target;

            if (fileProgress != progress.ItemProgress)
            {
                progress.ItemProgress = fileProgress;
            }

            CopyFileCallbackAction action = progress.CancellationToken.IsCancellationRequested ? CopyFileCallbackAction.CANCEL : CopyFileCallbackAction.CONTINUE;

            return ((int)action);

        }

        public bool copyFile(string source, string destination, IProgress progress)
        {

            GCHandle handle = GCHandle.Alloc(progress);
            IntPtr progressPtr = GCHandle.ToIntPtr(handle);

            try
            {

                progress.ItemInfo = "Copying: " + source + " -> " + destination;

                FileIOPermission sourcePermission = new FileIOPermission(FileIOPermissionAccess.Read, source);
                sourcePermission.Demand();

                FileIOPermission destinationPermission = new FileIOPermission(
                    FileIOPermissionAccess.Write, destination);
                destinationPermission.Demand();

                string destinationDir = System.IO.Path.GetDirectoryName(destination);

                if (!Directory.Exists(destinationDir))
                {

                    Directory.CreateDirectory(destinationDir);                  
                }

                CopyProgressDelegate progressCallback = new CopyProgressDelegate(copyProgress);

                int cancel = 0;

                if (!CopyFileEx(source, destination, progressCallback,
                    progressPtr, ref cancel, (int)CopyFileOptions.ALL))
                {

                    Win32Exception win32exception = new Win32Exception();

                    if (win32exception.NativeErrorCode == 1235)
                    {

                        // copy was cancelled
                        return (false);
                    }

                    throw new IOException(win32exception.Message);
                }

                progress.InfoMessages.Add("Copied: " + source + " -> " + destination);

                return (true);

            }
            finally
            {

                handle.Free();
            }
        }

        public void copy(StringCollection sourcePaths, StringCollection destPaths, IProgress progress)
        {
            if (sourcePaths.Count != destPaths.Count)
            {

                throw new System.ArgumentException();
            }

            try
            {

                StringCollection allSourcePaths;
                StringCollection allDestPaths;
                StringCollection createDirs;
                StringCollection removeDirs;

                getPaths(sourcePaths, destPaths, out allSourcePaths, out allDestPaths, out createDirs, out removeDirs);

                progress.TotalProgressMax = allSourcePaths.Count - 1;              
         
                foreach (String directory in createDirs)
                {
                    Directory.CreateDirectory(directory);
                }

                for (int i = 0; i < allSourcePaths.Count; i++)
                {

                    progress.TotalProgress = i;

                    bool success = copyFile(allSourcePaths[i], allDestPaths[i], progress);               

                    if (success == false) break;
                                     
                }            

            }
            catch (Exception e)
            {
                log.Error("Copy Exception", e);
                progress.ItemInfo = "Copy Exception: " + e.Message;
            }            

        }
       
        public void moveFile(string source, string destination, IProgress progress)
        {
            if (progress.CancellationToken.IsCancellationRequested) return;
            
            if (source.Equals(destination))
            {
                return;

            } else if (Path.GetPathRoot(source).Equals(Path.GetPathRoot(destination)))
            {
                progress.ItemProgress = 0;
                progress.ItemInfo = "Moving: " + source + "->" + destination;

                System.IO.Directory.Move(source, destination);

                progress.InfoMessages.Add("Moved: " + source + " -> " + destination);
                progress.ItemProgress = progress.ItemProgressMax;
            }
            else
            {
                FileInfo sourceFile = new FileInfo(source);
                if (sourceFile.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    throw new System.IO.IOException("Cannot move readonly file");
                }

                copyFile(source, destination, progress);

                System.IO.File.Delete(source);
                progress.InfoMessages.Add("Deleted: " + source);
            }

        }

        public void move(StringCollection sourcePaths, StringCollection destPaths, IProgress progress)
        {

            if (progress.CancellationToken.IsCancellationRequested) return;

            if (sourcePaths.Count != destPaths.Count)
            {
                throw new System.ArgumentException();
            }

            try
            {

                StringCollection allSourcePaths;
                StringCollection allDestPaths;
                StringCollection createDirs;
                StringCollection removeDirs;

                getPaths(sourcePaths, destPaths, out allSourcePaths, out allDestPaths, out createDirs, out removeDirs);

                progress.TotalProgressMax = allSourcePaths.Count - 1;

                foreach (String directory in createDirs)
                {
                    Directory.CreateDirectory(directory);
                }

                for (int i = 0; i < allSourcePaths.Count; i++)
                {
                    if (progress.CancellationToken.IsCancellationRequested) return;

                    progress.TotalProgress = i;

                    moveFile(allSourcePaths[i], allDestPaths[i], progress);
                }

                foreach (String directory in removeDirs)
                {
                    Directory.Delete(directory);
                }

            }
            catch (Exception e)
            {

                log.Error("Move Exception", e);
                progress.ItemInfo = "Move Exception: " + e.Message;
            }
         

        }

        void getPaths(StringCollection sourcePaths, StringCollection destPaths, 
           out StringCollection allSourcePaths, out StringCollection allDestPaths,
           out StringCollection createDirs, out StringCollection removeDirs)
        {
            allSourcePaths = new StringCollection();
            allDestPaths = new StringCollection();
            removeDirs = new StringCollection();
            createDirs = new StringCollection();

            for (int i = 0; i < sourcePaths.Count; i++)
            {

                if (Directory.Exists(sourcePaths[i]))
                {
                    // file is a directory, get all it's subdirectories and files
                    StringCollection extraPaths = new StringCollection();

                    getAllFiles(sourcePaths[i], removeDirs, extraPaths);

                    foreach (string extraPath in extraPaths)
                    {
                        if (addIfNotExists(extraPath, allSourcePaths))
                        {

                            string postfix = extraPath.Remove(0, sourcePaths[i].Length);

                            string destPath = destPaths[i] + postfix;

                            allDestPaths.Add(destPath);
                        }
                    }

                    foreach (string removeDir in removeDirs)
                    {
                        createDirs.Add(destPaths[i] + "/" + Path.GetFileName(removeDir));
                    }

                }
                else
                {
                    if (addIfNotExists(sourcePaths[i], allSourcePaths))
                    {
                        allDestPaths.Add(destPaths[i]);
                    }
                }
            }

        }

        void getAllFiles(string path, StringCollection directories, StringCollection files)
        {

            string[] directoriesInPath = Directory.GetDirectories(path);

            foreach (string directory in directoriesInPath)
            {

                getAllFiles(directory, directories, files);

                if (directories != null)
                {
                    directories.Add(directory);
                }
            }

            if (files == null) return;

            string[] filesInPath = Directory.GetFiles(path);

            foreach (string file in filesInPath)
            {

                files.Add(file);
            }

        }

        bool addIfNotExists(string path, StringCollection paths)
        {

            bool containsPath = paths.Contains(path);

            if (!containsPath)
            {

                paths.Add(path);

            }

            return (!containsPath);
        }

        static bool isFileLockedException(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }



        public delegate bool WalkDirectoryTreeDelegate(FileInfo info, Object state);

        public delegate void FileUtilsDelegate(System.Object sender, FileUtilsEventArgs e);

        public FileUtils()
        {


        }


        public void deleteDirectory(String location)
        {
            StringCollection files = new StringCollection();
            StringCollection directories = new StringCollection();

            getAllFiles(location, directories, files);

            foreach (String file in files)
            {
                deleteFile(file);
            }

            System.IO.Directory.Delete(location, true);
        }
                
        public void deleteFile(String location)
        {
           
            System.IO.File.Delete(location);
            
        }

        public static Stream waitForFileAccess(string filePath, FileAccess access, int timeoutMs,
            CancellationToken token)
        {
            IntPtr fHandle;
            int errorCode;
            DateTime start = DateTime.Now;

            uint desiredAccess = (uint)CreateFileAccess.GENERIC_READ;
            int fileShare = (int)CreateFileShare.FILE_SHARE_READ;

            if (access == FileAccess.Write || access == FileAccess.ReadWrite)
            {

                desiredAccess |= (int)CreateFileAccess.GENERIC_WRITE;
                fileShare = (int)CreateFileShare.NONE;
            }

            int creationDisposition = (int)CreateFileCreationDisposition.OPEN_EXISTING;
            int fileAttributes = (int)(CreateFileAttributes.NORMAL | CreateFileAttributes.FILE_FLAG_RANDOM_ACCESS);

            while (true)
            {

                fHandle = CreateFileW(filePath, desiredAccess, fileShare, IntPtr.Zero,
                    creationDisposition, fileAttributes, IntPtr.Zero);

                errorCode = Marshal.GetLastWin32Error();

                if (fHandle != IntPtr.Zero && fHandle.ToInt64() != -1L)
                {

                    Microsoft.Win32.SafeHandles.SafeFileHandle handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(fHandle, true);

                    return new FileStream(handle, access);

                }
               
                if (errorCode != ERROR_SHARING_VIOLATION)
                    break;
                if (timeoutMs >= 0 && (DateTime.Now - start).TotalMilliseconds > timeoutMs)
                    break;
                if (token != null && token.IsCancellationRequested == true)
                    break;
                Thread.Sleep(100);
            }


            Win32Exception e = new System.ComponentModel.Win32Exception(errorCode);

            throw new IOException(e.Message, errorCode);
        }


        /*
            static FileStream openAndLockFile(string fileName, FileAccess fileAccess, FileShare fileShare, int maxAttempts, int timeoutMs, bool ignoreIOExceptions) {

                int attempt = 0;

                while(true) {

                    try {

                        return File.Open(fileName, FileMode.Open, fileAccess, fileShare); 

                    } catch(IOException e) {

                        if(!isFileLockedException(e)) {

                            // file is not locked but some other exception happend
                            // rethrow the exception

                            if(ignoreIOExceptions == false) {
                                throw;
                            }
                        }

                        if(++attempt > maxAttempts) {

                            // attempts to open file exceeded
                            throw;
                        }

                        Thread.Sleep(timeoutMs);

                    }
                }
            }
        */
        public static bool isFileLocked(string fileName, bool ignoreIOExceptions)
        {

            FileStream stream = null;

            try
            {

                stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None);

            }
            catch (IOException e)
            {

                if (isFileLockedException(e))
                {

                    return (true);
                }

                // file is not locked but some other exception happend
                // rethrow the exception
                if (ignoreIOExceptions == false)
                {
                    throw;
                }

            }
            finally
            {

                if (stream != null)
                {

                    stream.Close();
                }
            }

            return (false);
        }

        public static string getUniqueFileName(string fileName)
        {

            string uniqueName = fileName;
            string dir = Path.GetDirectoryName(fileName);
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);

            int i = 0;

            while (File.Exists(uniqueName))
            {

                uniqueName = dir + "\\" + name + " (" + Convert.ToString(++i) + ")" + ext;

            }

            return (uniqueName);
        }

        public static void walkDirectoryTree(DirectoryInfo root,
            WalkDirectoryTreeDelegate callback, Object state, bool recurseSubDirs)
        {
            if (callback == null) return;

            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                log.Warn(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                log.Warn(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo info in files)
                {
                    // In this example, we only access the existing FileInfo object. If we 
                    // want to open, delete or modify the file, then 
                    // a try-catch block is required here to handle the case 
                    // where the file has been deleted since the call to TraverseTree().
                    bool cont = callback.Invoke(info, state);
                    if (cont == false)
                    {
                        return;
                    }
                }

                // Now find all the subdirectories under this directory.
                if (recurseSubDirs == true)
                {

                    subDirs = root.GetDirectories();

                    foreach (DirectoryInfo dirInfo in subDirs)
                    {
                        // Resursive call foreach subdirectory.
                        walkDirectoryTree(dirInfo, callback, state, recurseSubDirs);
                    }
                }
            }
        }

        public static bool isUrl(string path)
        {

            if (path.StartsWith("http://") || path.StartsWith("https://"))
            {

                return (true);

            }
            else
            {

                return (false);
            }
        }

        public static string getProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            DirectoryInfo parentDirInfo = dirInfo.Parent;
            if (parentDirInfo == null)
            {

                return dirInfo.Name;
            }

            return Path.Combine(getProperDirectoryCapitalization(parentDirInfo),
                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        public static string getProperFilePathCapitalization(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;

            string result = Path.Combine(getProperDirectoryCapitalization(dirInfo),
                dirInfo.GetFiles(fileInfo.Name)[0].Name);

            return (Char.ToUpper(result[0]) + result.Substring(1));
        }


        public static string getPathWithoutFileName(string fullPath)
        {

            string fileName = System.IO.Path.GetFileName(fullPath);

            if (string.IsNullOrEmpty(fileName)) return (fullPath);

            return (fullPath.Remove(fullPath.Length - fileName.Length - 1));
        }

        public static string removeIllegalCharsFromFileName(string fileName, string replaceString)
        {

            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return (r.Replace(fileName, replaceString));

        }

        public static FileSystemRights getRights(string userName, string path)
        {

            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException("userName");
            }

            if (!Directory.Exists(path) && !File.Exists(path))
            {
                throw new ArgumentException(string.Format("path:  {0}", path));
            }

            return getEffectiveRights(userName, path);

        }



        private static FileSystemRights getEffectiveRights(string userName, string path)
        {

            FileSystemAccessRule[] accessRules = getAccessRulesArray(userName, path);

            FileSystemRights denyRights = 0;
            FileSystemRights allowRights = 0;

            for (int index = 0, total = accessRules.Length; index < total; index++)
            {

                FileSystemAccessRule rule = accessRules[index];

                if (rule.AccessControlType == AccessControlType.Deny)
                {
                    denyRights |= rule.FileSystemRights;
                }

                else
                {
                    allowRights |= rule.FileSystemRights;
                }

            }

            return (allowRights | denyRights) ^ denyRights;

        }

        private static FileSystemAccessRule[] getAccessRulesArray(string userName, string path)
        {

            // get all access rules for the path - this works for a directory path as well as a file path
            AuthorizationRuleCollection authorizationRules = (new FileInfo(path)).GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));

            // get the user's sids
            string[] sids = getSecurityIdentifierArray(userName);

            // get the access rules filtered by the user's sids
            return (from rule in authorizationRules.Cast<FileSystemAccessRule>()
                    where sids.Contains(rule.IdentityReference.Value)
                    select rule).ToArray();

        }

        private static string[] getSecurityIdentifierArray(string userName)
        {

            // connect to the domain
            PrincipalContext pc = new PrincipalContext(ContextType.Domain);

            // search for the domain user
            UserPrincipal user = new UserPrincipal(pc) { SamAccountName = userName };
            PrincipalSearcher searcher = new PrincipalSearcher { QueryFilter = user };
            user = searcher.FindOne() as UserPrincipal;

            if (user == null)
            {
                throw new ApplicationException(string.Format("Invalid User Name:  {0}", userName));
            }

            // use WindowsIdentity to get the user's groups
            WindowsIdentity windowsIdentity = new WindowsIdentity(user.UserPrincipalName);

            string[] sids = new string[windowsIdentity.Groups.Count + 1];
            sids[0] = windowsIdentity.User.Value;

            for (int index = 1, total = windowsIdentity.Groups.Count; index < total; index++)
            {
                sids[index] = windowsIdentity.Groups[index].Value;
            }

            return sids;
        }

    }
}
