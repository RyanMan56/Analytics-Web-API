using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Messages
{    
    public static class ErrorMessages
    {
        public static string generic = "A problem happened while handling your request.";
        public static string save = "A problem happened while saving your request.";
        public static string projectNotFound = "The requested project doesn't exist.";
        public static string eventNotFound = "The requested event doesn't exist.";
        public static string userNotAnalyser = "User is not analyser of this project.";
    }       
}
