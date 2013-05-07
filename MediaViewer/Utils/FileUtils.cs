using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MediaViewer.GenericEventArgs;

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

        private class AsyncState
        {

            private void progressForm_CancelEvent(Object sender, EventArgs e)
            {
                copyCallbackAction = CopyFileCallbackAction.CANCEL;
            }

            public StringCollection sourcePaths;
            public StringCollection destPaths;
            public ProgressWindow ProgressWindow;

            public CopyFileCallbackAction copyCallbackAction;

            public AsyncState(StringCollection sourcePaths, StringCollection destPaths)
            {

                this.sourcePaths = sourcePaths;
                this.destPaths = destPaths;
                ProgressWindow = new ProgressWindow();
                ProgressWindow.CancelEvent += new ProgressWindow.CancelEventHandler(progressForm_CancelEvent);

                copyCallbackAction = CopyFileCallbackAction.CONTINUE;
            }


        };

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
            AsyncState state = (AsyncState)h.Target;

            state.ProgressWindow.ItemProgressValue = fileProgress;

            return ((int)state.copyCallbackAction);

        }

        bool copyFile(string source, string destination, CopyFileOptions options, AsyncState state)
        {

            GCHandle handle = GCHandle.Alloc(state);
            IntPtr statePtr = GCHandle.ToIntPtr(handle);

            try
            {

                FileIOPermission sourcePermission = new FileIOPermission(FileIOPermissionAccess.Read, source);
                sourcePermission.Demand();

                FileIOPermission destinationPermission = new FileIOPermission(
                    FileIOPermissionAccess.Write, destination);
                destinationPermission.Demand();

                string destinationDir = System.IO.Path.GetDirectoryName(destination);

                if (!Directory.Exists(destinationDir))
                {

                    Directory.CreateDirectory(destinationDir);
                    OnAfterCopy(this, new FileUtilsEventArgs(destinationDir, true));
                }

                CopyProgressDelegate progressCallback = new CopyProgressDelegate(copyProgress);

                int cancel = 0;

                if (!CopyFileEx(source, destination, progressCallback,
                    statePtr, ref cancel, (int)options))
                {

                    Win32Exception win32exception = new Win32Exception();

                    if (win32exception.NativeErrorCode == 1235)
                    {

                        // copy was cancelled
                        return (false);
                    }

                    throw new IOException(win32exception.Message);
                }

                return (true);

            }
            finally
            {

                handle.Free();
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


        void asyncCopy(Object args)
        {

            AsyncState state = (AsyncState)args;

            try
            {

                StringCollection sourcePaths = state.sourcePaths;
                StringCollection destPaths = state.destPaths;

                StringCollection movePaths = new StringCollection();
                StringCollection moveDestPaths = new StringCollection();

                StringCollection copyPaths = new StringCollection();
                StringCollection copyDestPaths = new StringCollection();

                StringCollection removePaths = new StringCollection();

                getPaths(sourcePaths, destPaths, movePaths, moveDestPaths, copyPaths, copyDestPaths, removePaths, Action.COPY);

                state.ProgressWindow.TotalProgressMaximum = movePaths.Count + copyPaths.Count;

                bool success = true;

                for (int i = 0; i < movePaths.Count; i++)
                {

                    state.ProgressWindow.TotalProgressValue = i;
                    state.ProgressWindow.ItemInfo = movePaths[i];

                    success = copyFile(movePaths[i], moveDestPaths[i], CopyFileOptions.ALL, state);

                    if (success == false) break;

                    bool isDirectory = Directory.Exists(moveDestPaths[i]);

                    OnAfterCopy(this, new FileUtilsEventArgs(moveDestPaths[i], isDirectory));

                    //state.ProgressWindow.addInfoString(movePaths[i] + " . " + moveDestPaths[i]);
                }


                for (int i = 0; (i < copyPaths.Count) && (success == true); i++)
                {

                    state.ProgressWindow.TotalProgressValue = movePaths.Count + i;
                    state.ProgressWindow.ItemInfo = copyPaths[i];

                    success = copyFile(copyPaths[i], copyDestPaths[i], CopyFileOptions.ALL, state);

                    if (success == false) break;

                    OnAfterCopy(this, new FileUtilsEventArgs(copyDestPaths[i], false));

                    //state.ProgressWindow.addInfoString(copyPaths[i] + " . " + copyDestPaths[i]);

                }

                if (success == true)
                {

                    state.ProgressWindow.TotalProgressValue = state.ProgressWindow.TotalProgressMaximum;
                }

            }
            catch (Exception e)
            {

                log.Error("Copy Exception", e);
                MessageBox.Show(e.Message, "Copy Exception");
            }

            state.ProgressWindow.actionFinished();

        }

        void getPaths(StringCollection sourcePaths, StringCollection destPaths,
            StringCollection movePaths, StringCollection moveDestPaths,
            StringCollection copyPaths, StringCollection copyDestPaths,
            StringCollection removePaths, Action action)
        {

            for (int i = 0; i < sourcePaths.Count; i++)
            {

                string sourceRoot = System.IO.Path.GetPathRoot(sourcePaths[i]);
                string destRoot = System.IO.Path.GetPathRoot(destPaths[i]);

                if (sourcePaths[i].Equals(destPaths[i]))
                {

                    if (action == Action.MOVE)
                    {

                        // don't do anything

                    }
                    else
                    {

                        if (Directory.Exists(sourcePaths[i])) continue;

                        // copy to unique filename
                        copyPaths.Add(sourcePaths[i]);
                        copyDestPaths.Add(getUniqueFileName(sourcePaths[i]));
                    }

                }
                else if (sourceRoot.Equals(destRoot))
                {

                    // files can be moved on the same drive
                    movePaths.Add(sourcePaths[i]);
                    moveDestPaths.Add(destPaths[i]);

                }
                else
                {

                    // files need to be copied between drives
                    if (Directory.Exists(sourcePaths[i]))
                    {

                        // file is a directory, get all it's subdirectories and files
                        StringCollection subPaths = new StringCollection();

                        addIfNotExists(sourcePaths[i], removePaths);

                        getAllFiles(sourcePaths[i], null, subPaths);

                        foreach (string subPath in subPaths)
                        {

                            if (addIfNotExists(subPath, copyPaths))
                            {

                                string postfix = subPath.Remove(0, sourcePaths[i].Length);

                                string destPath = destPaths[i] + postfix;

                                copyDestPaths.Add(destPath);
                            }
                        }

                    }
                    else
                    {


                        if (addIfNotExists(sourcePaths[i], copyPaths))
                        {

                            copyDestPaths.Add(destPaths[i]);
                        }
                    }
                }
            }
        }

        void asyncMove(Object args)
        {

            AsyncState state = (AsyncState)args;

            try
            {

                StringCollection sourcePaths = state.sourcePaths;
                StringCollection destPaths = state.destPaths;

                StringCollection movePaths = new StringCollection();
                StringCollection moveDestPaths = new StringCollection();

                StringCollection copyPaths = new StringCollection();
                StringCollection copyDestPaths = new StringCollection();

                StringCollection removePaths = new StringCollection();

                getPaths(sourcePaths, destPaths, movePaths, moveDestPaths, copyPaths, copyDestPaths, removePaths, Action.MOVE);

                state.ProgressWindow.TotalProgressMaximum = movePaths.Count + copyPaths.Count;

                for (int i = 0; i < movePaths.Count; i++)
                {

                    state.ProgressWindow.TotalProgressValue = i;
                    state.ProgressWindow.ItemInfo = movePaths[i];

                    System.IO.Directory.Move(movePaths[i], moveDestPaths[i]);

                    bool isDirectory = Directory.Exists(moveDestPaths[i]);

                    OnAfterCopy(this, new FileUtilsEventArgs(moveDestPaths[i], isDirectory));
                    OnAfterDelete(this, new FileUtilsEventArgs(movePaths[i], isDirectory));

                    //state.ProgressWindow.addInfoString(movePaths[i] + " . " + moveDestPaths[i]);
                }

                bool success = true;

                for (int i = 0; i < copyPaths.Count; i++)
                {

                    state.ProgressWindow.TotalProgressValue = movePaths.Count + i;
                    state.ProgressWindow.ItemInfo = copyPaths[i];

                    success = copyFile(copyPaths[i], copyDestPaths[i], CopyFileOptions.ALL, state);

                    if (success == false) break;

                    OnAfterCopy(this, new FileUtilsEventArgs(copyDestPaths[i], false));

                    //state.ProgressWindow.addInfoString(copyPaths[i] + " . " + copyDestPaths[i]);

                }

                if (success == true)
                {

                    state.ProgressWindow.TotalProgressValue = state.ProgressWindow.TotalProgressMaximum;

                    foreach (string copySource in copyPaths)
                    {

                        System.IO.File.Delete(copySource);
                        OnAfterDelete(this, new FileUtilsEventArgs(copySource, false));
                    }

                    foreach (string directorySource in removePaths)
                    {

                        System.IO.Directory.Delete(directorySource, true);
                        OnAfterDelete(this, new FileUtilsEventArgs(directorySource, true));
                    }
                }

            }
            catch (Exception e)
            {

                log.Error("Move Exception", e);
                MessageBox.Show(e.Message, "Move Exception");
            }

            state.ProgressWindow.actionFinished();

        }

        static bool isFileLockedException(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }



        public delegate void WalkDirectoryTreeDelegate(FileInfo info, Object state);

        public delegate void FileUtilsDelegate(System.Object sender, FileUtilsEventArgs e);
        public event FileUtilsDelegate OnAfterCopy;
        public event FileUtilsDelegate OnAfterDelete;

        public FileUtils()
        {


        }

        public void copy(StringCollection sourcePaths, StringCollection destPaths)
        {

            if (sourcePaths.Count != destPaths.Count)
            {

                throw new System.ArgumentException();
            }

            WaitCallback callback = new WaitCallback(asyncCopy);

            AsyncState args = new AsyncState(sourcePaths, destPaths);
            args.ProgressWindow.Title = "Copy Files";
            args.ProgressWindow.Show();

            ThreadPool.QueueUserWorkItem(callback, args);

        }

        public void move(StringCollection sourcePaths, StringCollection destPaths)
        {

            if (sourcePaths.Count != destPaths.Count)
            {

                throw new System.ArgumentException();
            }

            WaitCallback callback = new WaitCallback(asyncMove);

            AsyncState args = new AsyncState(sourcePaths, destPaths);
            args.ProgressWindow.Title = "Moving Files";
            args.ProgressWindow.Show();

            ThreadPool.QueueUserWorkItem(callback, args);

        }

        public void remove(StringCollection sourcePaths)
        {

        }

        public static Stream waitForFileAccess(string filePath, FileAccess access, int timeoutMs,
            ModifiableGEventArgs<bool> isCancelled)
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

                if (fHandle != IntPtr.Zero && fHandle.ToInt64() != -1L)
                {

                    Microsoft.Win32.SafeHandles.SafeFileHandle handle = new Microsoft.Win32.SafeHandles.SafeFileHandle(fHandle, true);

                    return new FileStream(handle, access);

                }

                errorCode = Marshal.GetLastWin32Error();

                if (errorCode != ERROR_SHARING_VIOLATION)
                    break;
                if (timeoutMs >= 0 && (DateTime.Now - start).TotalMilliseconds > timeoutMs)
                    break;
                if (isCancelled != null && isCancelled.Value == true)
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
                    callback.Invoke(info, state);
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

        static string getProperDirectoryCapitalization(DirectoryInfo dirInfo)
        {
            DirectoryInfo parentDirInfo = dirInfo.Parent;
            if (parentDirInfo == null)
            {

                return dirInfo.Name;
            }

            return Path.Combine(getProperDirectoryCapitalization(parentDirInfo),
                parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
        }

        static string getProperFilePathCapitalization(string filename)
        {
            FileInfo fileInfo = new FileInfo(filename);
            DirectoryInfo dirInfo = fileInfo.Directory;

            string result = Path.Combine(getProperDirectoryCapitalization(dirInfo),
                dirInfo.GetFiles(fileInfo.Name)[0].Name);

            return (Char.ToUpper(result[0]) + result.Substring(1));
        }


        static string getPathWithoutFileName(string fullPath)
        {

            string fileName = System.IO.Path.GetFileName(fullPath);

            if (string.IsNullOrEmpty(fileName)) return (fullPath);

            return (fullPath.Remove(fullPath.Length - fileName.Length - 1));
        }

        static string removeIllegalCharsFromFileName(string fileName)
        {

            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return (r.Replace(fileName, ""));

        }
    }
}
