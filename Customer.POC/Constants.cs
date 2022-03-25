namespace Customer.POC;

public static class Constants
{
    public static class CosmosDb
    {
        public static string databaseName = "playground";
        public static string containerName = "customer";
    }
    public static class Email
    {
        public static class CustomerCreated
        {
            public static string content = "Customer created - TEST";
            public static string subject = "SendGrid Test Email";
            public static string fromEmail = "coreen.oliva@bjss.com.au";
            public static string toEmail = "coreenoliva@gmail.com";
        }
    }
}