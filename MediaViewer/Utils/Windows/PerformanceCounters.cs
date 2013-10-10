using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Utils.Windows
{
    class PerformanceCounters
    {

/*
        public static void createIfNotExists(string categoryName, string counterName, string instanceName, string machineName,
            string categoryHelp, string counterHelp)
        {            
                    
            bool objectExists = false;
            PerformanceCounterCategory pcc;
            bool createCategory = false;
                 
            // Verify that the category name is not blank. 
            if (categoryName.Length == 0)
            {
                throw new ArgumentException("Category name cannot be blank.");            
            }

            // Check whether the specified category exists. 
            if (machineName.Length == 0)
            {
                objectExists = PerformanceCounterCategory.Exists(categoryName);
            }
            else
            {
                // Handle the exception that is thrown if the computer  
                // cannot be found. 
                try
                {
                    objectExists = PerformanceCounterCategory.Exists(categoryName, machineName);
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Error checking for existence of " +
                        "category \"{0}\" on computer \"{1}\":" + "\n" + ex.Message, categoryName, machineName));               
                }
            }

            // Tell the user whether the specified category exists.
            //Console.WriteLine("Category \"{0}\" " + (objectExists ? "exists on " : "does not exist on ") +
            //    (machineName.Length > 0 ? "computer \"{1}\"." : "this computer."), categoryName, machineName);

            // If no counter name is given, the program cannot continue. 
            if (counterName.Length == 0)
            {
                throw new ArgumentException("counterName name cannot be blank.");                
            }

            // A category can only be created on the local computer. 
            if (!objectExists)
            {
                if (machineName.Length > 0)
                {
                    throw new Exception("A category can only be created on the local computer.");      
                }
                else
                {
                    createCategory = true;
                }
            }
            else
            {
                // Check whether the specified counter exists. 
                if (machineName.Length == 0)
                {
                    objectExists = PerformanceCounterCategory.CounterExists(counterName, categoryName);
                }
                else
                {
                    objectExists = PerformanceCounterCategory.CounterExists(counterName, categoryName, machineName);
                }

                // Tell the user whether the counter exists.
               // Console.WriteLine("Counter \"{0}\" " + (objectExists ? "exists" : "does not exist") +
               //     " in category \"{1}\" on " + (machineName.Length > 0 ? "computer \"{2}\"." : "this computer."),
               //     counterName, categoryName, machineName);

                // If the counter does not exist, consider creating it. 
                if (!objectExists)

                // If this is a remote computer,  
                // exit because the category cannot be created.
                {
                    if (machineName.Length > 0)
                    {
                        throw new Exception("A category can only be created on the local computer.");  
                    }
                    else
                    {
                        
                        // Ask whether the user wants to recreate the category.
                        Console.Write("Do you want to delete and recreate " +
                            "category \"{0}\" with your new counter? [Y/N]: ", categoryName);
                        string userReply = Console.ReadLine();

                        // If yes, delete the category so it can be recreated later. 
                        if (userReply.Trim().ToUpper() == "Y")
                        {
                            PerformanceCounterCategory.Delete(categoryName);
                            createCategory = true;
                        }
                        else
                        {
                            return;
                        }
                         
                    }
                }
            }

            // Create the category if it was deleted or it never existed. 
            if (createCategory)
            {
                pcc = PerformanceCounterCategory.Create(categoryName, categoryHelp, counterName, counterHelp);

              //  Console.WriteLine("Category \"{0}\" with counter \"{1}\" created.", pcc.CategoryName, counterName);

            }
            else if (instanceName.Length > 0)
            {
                if (machineName.Length == 0)
                {
                    objectExists = PerformanceCounterCategory.InstanceExists(instanceName, categoryName);
                }
                else
                {
                    objectExists = PerformanceCounterCategory.InstanceExists(instanceName, categoryName, machineName);
                }

                // Tell the user whether the instance exists.
                //Console.WriteLine("Instance \"{0}\" " + (objectExists ? "exists" : "does not exist") +
                //    " in category \"{1}\" on " + (machineName.Length > 0 ? "computer \"{2}\"." : "this computer."),
                //    instanceName, categoryName, machineName);
            }
        }
*/


        public static void listCategories()
        {
            //Get all performance categories
            var cat = new System.Diagnostics.PerformanceCounterCategory("PhysicalDisk");
            var instNames = cat.GetInstanceNames();
        }

        public static void listCountersByCategory(string category)
        {
            PerformanceCounterCategory[] perfCats = PerformanceCounterCategory.GetCategories();

            //Get single category by category name.
            PerformanceCounterCategory cat = perfCats.Where(c => c.CategoryName == category).FirstOrDefault();
            Console.WriteLine("Category Name: {0}", cat.CategoryName);

            //Get all instances available for category
            string[] instances = cat.GetInstanceNames();
            if (instances.Length == 0)
            {
                //This block will execute when category has no instance.
                //loop all the counters available withing category
                foreach (PerformanceCounter counter in cat.GetCounters())
                    Console.WriteLine("     Counter Name: {0}", counter.CounterName);
            }
            else
            {
                //This block will execute when category has one or more instances.
                foreach (string instance in instances)
                {
                    Console.WriteLine("  Instance Name: {0}", instance);
                    if (cat.InstanceExists(instance))
                        //loop all the counters available withing category
                        foreach (PerformanceCounter counter in cat.GetCounters(instance))
                            Console.WriteLine("     Counter Name: {0}", counter.CounterName);
                }
            }
        }
    }
}
