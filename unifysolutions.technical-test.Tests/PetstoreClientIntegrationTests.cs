using unifysolutions.technical_test.Client;

namespace unifysolutions.technical_test.Tests
{
    [TestClass]
    public class PetstoreClientIntegrationTests
    {
        private readonly PetstoreClient _petstoreClient;

        public PetstoreClientIntegrationTests()
        {
            _petstoreClient = new PetstoreClient(new HttpClient());
        }

        [TestMethod]
        public async Task GetAvailablePetsOrderByNameAsync_ReturnsPetsInDescendingOrder_WhenPassedDescendingOrderParameter()
        {
            var result = await _petstoreClient.GetAvailablePetsOrderByNameAsync(SortOrder.Descending);
            Assert.That.CollectionIsInExpectedOrderBasedOnStringProperty(result, r=>r.Name, SortOrder.Descending);
        }

        [TestMethod]
        public async Task GetAvailablePetsOrderByNameAsync_ReturnsPetsInAscendingOrder_WhenPassedAscendingOrderParameter()
        {
            var result = await _petstoreClient.GetAvailablePetsOrderByNameAsync(SortOrder.Ascending);
            Assert.That.CollectionIsInExpectedOrderBasedOnStringProperty(result, r => r.Name, SortOrder.Ascending);
        }

        [TestMethod]
        public async Task GetAvailablePetsOrderByNameAsync_ReturnsPetsWithStatusOfAvailableOnly()
        {
            var result = await _petstoreClient.GetAvailablePetsOrderByNameAsync(SortOrder.Ascending);

            Assert.IsTrue(result.All(pet => pet.Status == "available"));
        }
    }
}