# Unify Solutions: Technical Test

## Chat GPT Transcript:

**Prompt** As a software engineer produce a .net client api call with retry logic to get the list of all pets using the api documented here https://petstore.swagger.io

**Answer** As a software engineer produce a .net client api call with retry logic to get the list of all pets using the api documented here https://petstore.swagger.io

	using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Polly;

    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            await retryPolicy.ExecuteAsync(async () =>
            {
                HttpResponseMessage response = await client.GetAsync("https://petstore.swagger.io/v2/pet/findByStatus?status=available");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseBody);
            });
        }
    }

This code uses the Polly library to implement a simple exponential backoff retry policy. If the GetAsync call fails with an HttpRequestException, Polly will wait for a period of time and then retry the call. The WaitAndRetryAsync method takes two parameters: the number of times to retry the operation, and a function that determines how long to wait between retries. In this case, it waits for 2 to the power of the retry attempt number in seconds between retries.

Please note that you’ll need to install the Polly NuGet package to your project to use this code. You can do this by running the following command in your Package Manager Console:

    Install-Package Polly

Also, this is a very basic example. In a real-world application, you’d likely want to improve this by adding more sophisticated error handling and possibly combining multiple Polly policies (for example, to handle timeouts or circuit breaking).

**Prompt** Create a pets model class based off the response object from the v2/pet/findByStatus at https://petstore.swagger.io

**Answer** Create a pets model class based off the response object from the v2/pet/findByStatus at https://petstore.swagger.io

    public class Pet
    {
        public long Id { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public List<string> PhotoUrls { get; set; }
        public List<Tag> Tags { get; set; }
        public string Status { get; set; }
    }

    public class Category
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Tag
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

This is a simplified model and might not cover all properties provided by the API. You should adjust it according to the actual response from the API. Also, please note that error handling and null checking are not included in this example and should be implemented as needed.

Please note that the Status property in the Pet class is a string. According to the API documentation, it should have one of the following values: “available”, “pending”, or “sold”. You might want to use an enumeration for this property to make your code safer and more readable. However, this would also require additional code to correctly serialize and deserialize this property to and from JSON.

Also, please note that the PhotoUrls property is a list of strings. According to the API documentation, it should contain URLs of photos of the pet. Depending on your application, you might want to use a different data type for this property. For example, if you want to download the photos and store them locally, you might want to use a list of Image objects instead of a list of strings.

Finally, please note that the Category and Tags properties are complex types, represented by the Category and Tag classes. According to the API documentation, a category is a grouping of pets, and a tag is a label attached to a pet. Depending on your application, you might want to add additional properties to these classes, or you might want to use a different data structure to represent these concepts. For example, if categories and tags have additional attributes in your application, you might want to add additional properties to the Category and Tag classes. If categories and tags are used in a way that is not well-represented by a class, you might want to use a different data structure, such as a dictionary or a set.

**Prompt** How to pass a propertyinfo into a method through expressions using the syntax p=>p.Name in .net if you want to get the propertyInfo of property 'Name' on the object 'p'

**Answer** In .NET, you can pass a PropertyInfo into a method using expressions by utilizing the Expression<Func<T, object>> syntax. This allows you to specify a lambda expression that references a property on an object, and then extract the PropertyInfo from this expression within the method. Here's a step-by-step guide on how to do this:

1. Define the Method: First, define a method that accepts an expression as a parameter. This expression represents a function that takes an object of type T and returns a property of this object.
        
       public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> propertyLambda)
       {
           // Method implementation will go here
       }

2. Extract MemberInfo from the Expression: Within the method, you need to extract the MemberInfo from the provided expression. The expression's body might be a UnaryExpression (if the property is of a value type) or a MemberExpression (if the property is of a reference type).

        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> propertyLambda)
        {
            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                var unaryExpression = propertyLambda.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    member = unaryExpression.Operand as MemberExpression;
                }
            }

            if (member == null)
            {
                throw new ArgumentException("The lambda expression 'propertyLambda' should point to a valid property");
            }

            return member.Member as PropertyInfo;
        }        

3. Using the Method: You can now use this method to get the PropertyInfo of a property. For example, if you have a class Person with a property Name, you can get the PropertyInfo of Name like this:
        
        public class Person
        {
            public string Name { get; set; }
        }
        PropertyInfo propertyInfo = GetPropertyInfo<Person>(p => p.Name);

4.Handling Different Types: This method handles both reference and value type properties due to the inclusion of the UnaryExpression check. If the property is a value type, the expression body will be a UnaryExpression, and the actual MemberExpression will be the operand of this unary expression.

Conclusion: This approach allows you to neatly pass property information using simple lambda expressions, enhancing the readability and maintainability of your code. It can be particularly useful in scenarios like implementing INotifyPropertyChanged, creating generic data access layers, or building dynamically-typed queries.

## Changes for enterprise:

1. PetstoreClient Changes:
    - Ceate abstract base class to handle logic that could be shared by multiple 'clients' such as retry logic
    - Create interface and define public API on that interface to make it simple to replace the client if needed
    - Commenting on the public API 
    - Pass in ILogger to allow logging of errors
    - Investigate using IAsyncEnumerables to stream the data back to the client to improve time to first repsonse for the user
    - Add a PetValidator class that is passed into the client. This class would be responsible for validating the data returned from the api is what is expected
    - Turn the status code into an Enum and refactor GetAvailablePetsAsync to GetPetsAsync(Status status, CancellationToken cancellationToken = default)
    - Move endpoint strings to constants all located in one area of the file
    - Use available cancellation tokens to provide a way to stop api calls if they are failling to complete or need to be stopped by the calling application for any reason
2. Models:
    - Commenting on the properties and classes in the model folder
3. Tests:
    - Move Extensions class to separate library which would also have unit tests
    - Investigate supporting more than just string based properties in the Extesnion method
    - Commenting in the extension class
    - Use separate enum in the extension class to what is used in the Petstore client as this ties the two unrelated objects together
4. General:
    - Enquire about the use of a console application. Is this really the desired way the enterprise wants to retrieve this data?
    - Use built in .Net IOC container to inject required components