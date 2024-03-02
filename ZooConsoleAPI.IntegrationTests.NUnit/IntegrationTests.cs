using NUnit.Framework;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using ZooConsoleAPI.Business;
using ZooConsoleAPI.Business.Contracts;
using ZooConsoleAPI.Data.Model;
using ZooConsoleAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ZooConsoleAPI.IntegrationTests.NUnit
{
    public class IntegrationTests
    {
        private TestAnimalDbContext dbContext;
        private IAnimalsManager animalsManager;

        [SetUp]
        public void SetUp()
        {
            this.dbContext = new TestAnimalDbContext();
            this.animalsManager = new AnimalsManager(new AnimalRepository(this.dbContext));
        }


        [TearDown]
        public void TearDown()
        {
            this.dbContext.Database.EnsureDeleted();
            this.dbContext.Dispose();
        }


        //positive test
        [Test]
        public async Task AddAnimalAsync_ShouldAddNewAnimal()
        {
            // Arrange
            var newAnimal = new Animal()
           {
               CatalogNumber = "01HNTWXTQSH4",
               Name = "Lappy",
               Breed = "Baboon, savanna",
               Type = "Mammal",
               Age = 2,
               Gender = "Female",
               IsHealthy = false,
           };
            await animalsManager.AddAsync(newAnimal);

            // Act
            var dbAnimal = await this.dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);

            // Assert
            Assert.NotNull(dbAnimal);
            Assert.AreEqual(dbAnimal.CatalogNumber, dbAnimal.CatalogNumber);
            Assert.AreEqual(dbAnimal.Name, dbAnimal.Name);
            Assert.AreEqual(dbAnimal.Breed, dbAnimal.Breed);
            Assert.AreEqual(dbAnimal.Type, dbAnimal.Type);
            Assert.AreEqual(dbAnimal.Age, dbAnimal.Age);
            Assert.AreEqual(dbAnimal.Gender, dbAnimal.Gender);
            Assert.AreEqual(dbAnimal.IsHealthy, dbAnimal.IsHealthy);

        }

        //Negative test
        [Test]
        public async Task AddAnimalAsync_TryToAddAnimalWithInvalidCredentials_ShouldThrowException()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = -2,
                Gender = "Female",
                IsHealthy = false,
            };
            //Act
            var exeption = Assert.ThrowsAsync<ValidationException>(async () => await animalsManager.AddAsync(newAnimal));
            var actual = await dbContext.Animals.FirstOrDefaultAsync(c => c.CatalogNumber == newAnimal.CatalogNumber);
            //Assert
            Assert.IsNull(actual);
            Assert.That(exeption.Message, Is.EqualTo("Invalid animal!"));


        }

        [Test]
        public async Task DeleteAnimalAsync_WithValidCatalogNumber_ShouldRemoveAnimalFromDb()
        {
            // Arrange
            var newAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(newAnimal);
            // Act
            await animalsManager.DeleteAsync(newAnimal.CatalogNumber);
            // Assert
            var animalInDB = await dbContext.Animals.FirstOrDefaultAsync(a => a.CatalogNumber == newAnimal.CatalogNumber);
            Assert.IsNull(animalInDB);


        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]

        public async Task DeleteAnimalAsync_TryToDeleteWithNullOrWhiteSpaceCatalogNumber_ShouldThrowException(string InvalideCode)
        {
            // Act
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => animalsManager.DeleteAsync(InvalideCode));
            // Assert
            Assert.That(exeption.Message, Is.EqualTo("Catalog number cannot be empty."));

        }

        [Test]
        public async Task GetAllAsync_WhenAnimalsExist_ShouldReturnAllAnimals()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(firstAnimal);

            var secondAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(secondAnimal);
            // Act

            var result = await animalsManager.GetAllAsync();
            // Assert
            Assert.IsNotNull(firstAnimal);
            Assert.IsNotNull(secondAnimal);
            Assert.That(result.Count, Is.EqualTo(2));

        }

        [Test]
        public async Task GetAllAsync_WhenNoAnimalsExist_ShouldThrowKeyNotFoundException()
        {


            // Act and Assert
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(async () => await animalsManager.GetAllAsync());
            Assert.That(exeption.Message, Is.EqualTo("No animal found."));
        }

        [Test]
        public async Task SearchByTypeAsync_WithExistingType_ShouldReturnMatchingAnimals()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(firstAnimal);
            // Act
            var result = await animalsManager.SearchByTypeAsync(firstAnimal.Type);
            var resultAnimal = result.First();
            Assert.That(resultAnimal.Type, Is.EqualTo(firstAnimal.Type));
        }

        [Test]
        public async Task SearchByTypeAsync_WithNonExistingType_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.SearchByTypeAsync("InvalideType"));
            Assert.That(exeption.Message, Is.EqualTo("No animal found with the given type."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]

        public async Task SearchByTypeAsync_WithNullOrEmptyType_ShouldThrowArgumentException(string InvalideCode)
        {
            // Act and Assert
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => animalsManager.SearchByTypeAsync(InvalideCode));
            Assert.That(exeption.Message, Is.EqualTo("Animal type cannot be empty."));
        }

        [Test]
        public async Task GetSpecificAsync_WithValidCatalogNumber_ShouldReturnAnimal()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(firstAnimal);
            // Act
            var result = await animalsManager.GetSpecificAsync(firstAnimal.CatalogNumber);
            // Assert
            Assert.That(result.CatalogNumber, Is.EqualTo(firstAnimal.CatalogNumber));
        }

        [Test]
        public async Task GetSpecificAsync_WithInvalidCatalogNumber_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            var exeption = Assert.ThrowsAsync<KeyNotFoundException>(() => animalsManager.GetSpecificAsync("invalidNumber"));
            Assert.That(exeption.Message, Is.EqualTo($"No animal found with catalog number: {"invalidNumber"}"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]

        public async Task GetSpecificAsync_WithNullOrEmptyCatalogNumber_ShouldThrowArgumentException(string InvalideCode)
        {
            // Act and Assert
            var exeption = Assert.ThrowsAsync<ArgumentException>(() => animalsManager.GetSpecificAsync(InvalideCode));
            Assert.That(exeption.Message, Is.EqualTo("Catalog number cannot be empty."));
        }

        [Test]
        public async Task UpdateAsync_WithValidAnimal_ShouldUpdateAnimal()
        {
            // Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "01HNTWXTQSH4",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            await animalsManager.AddAsync(firstAnimal);
            // Act
            firstAnimal.CatalogNumber = "01HNTWXUPDET";
            await animalsManager.UpdateAsync(firstAnimal);

            // Assert
            var result = await animalsManager.GetSpecificAsync(firstAnimal.CatalogNumber);
            Assert.That(result.CatalogNumber, Is.EqualTo(firstAnimal.CatalogNumber));

           
        }

        [Test]
        public async Task UpdateAsync_WithInvalidAnimal_ShouldThrowValidationException()
        {
            //Arrange
            var firstAnimal = new Animal()
            {
                CatalogNumber = "01H",
                Name = "Lappy",
                Breed = "Baboon, savanna",
                Type = "Mammal",
                Age = 2,
                Gender = "Female",
                IsHealthy = false,
            };
            // Act and Assert
            var exeption = Assert.ThrowsAsync<ValidationException>(() => animalsManager.UpdateAsync(firstAnimal));
            Assert.That(exeption.Message, Is.EqualTo("Invalid animal!"));
        }

        [Test]
        public async Task UpdateAsync_WithNulldAnimal_ShouldThrowValidationException()
        {
          
            // Act and Assert
            var exeption = Assert.ThrowsAsync<ValidationException>(() => animalsManager.UpdateAsync(null));
            Assert.That(exeption.Message, Is.EqualTo("Invalid animal!"));
        }
    }
}

