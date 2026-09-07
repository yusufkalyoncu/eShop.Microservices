using BuildingBlocks.Core.Results;

namespace Catalog.API.Domain.Errors;

public static class CatalogErrors
{
    public static class Product
    {
        public static readonly Error NotFound = Error.NotFound(
            "Catalog.Product.NotFound",
            "The product with the specified identifier was not found.");
            
        public static readonly Error AlreadyExists = Error.Conflict(
            "Catalog.Product.AlreadyExists",
            "A product with the same attributes already exists.");
    }

    public static class Category
    {
        public static readonly Error NotFound = Error.NotFound(
            "Catalog.Category.NotFound",
            "The category with the specified identifier was not found.");
    }
}