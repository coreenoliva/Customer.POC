namespace Customer.POC;

public static class Constants
{
    public static class CosmosDb
    {
        public static string databaseName = "playground";
        public static string containerName = "customer";
    }

    public static class ActivityNames
    {
        public static string validator = "Activity_Validator";
        public static string createCustomerThirdParty = "Activity_CreateCustomer_ThirdParty";
        public static string createCustomerCosmosDb = "Activity_CreateCustomer_CosmosDb";
        public static string sendEmailCustomer = "Activity_SendEmail_Customer";
    }
    
    public static class ErrorMessages
    {
        public static string requestInvalid = "Request invalid";
        public static string unableToCreateCustomerThirdParty = "Unable to create customer in third party API";
        public static string unhandledException = "Unhandled exception while creating customer";
        public static string cosmosDbCustomerCreation = "Unable to create customer in cosmos db";
        public static string unableToSendEmail = "Error sending customer created email";

    }
    public static class Email
    {
        public static class CustomerCreated
        {
            public static string content = "Customer created - TEST";
            public static string subject = "SendGrid Test Email";
            public static string fromEmail = "coreen.oliva@bjss.com";
            public static string toEmail = "coreenoliva@gmail.com";
            public static string testUser = "Test User";
        }
    }
}