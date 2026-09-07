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

    public static class Money
    {
        public static readonly Error NegativeAmount = Error.Validation(
            "Catalog.Money.NegativeAmount",
            "Amount cannot be negative.");

        public static readonly Error EmptyCurrency = Error.Validation(
            "Catalog.Money.EmptyCurrency",
            "Currency cannot be empty.");
    }

    public static class ProductName
    {
        public static readonly Error Empty = Error.Validation(
            "Catalog.ProductName.Empty",
            "Product name cannot be empty.");

        public static readonly Error TooShort = Error.Validation(
            "Catalog.ProductName.TooShort",
            "Product name must be at least 3 characters long.");

        public static readonly Error TooLong = Error.Validation(
            "Catalog.ProductName.TooLong",
            "Product name cannot exceed 100 characters.");
    }

    public static class ProductDescription
    {
        public static readonly Error Empty = Error.Validation(
            "Catalog.ProductDescription.Empty",
            "Product description cannot be empty.");

        public static readonly Error TooLong = Error.Validation(
            "Catalog.ProductDescription.TooLong",
            "Product description cannot exceed 500 characters.");
    }

    public static class CategoryName
    {
        public static readonly Error Empty = Error.Validation(
            "Catalog.CategoryName.Empty",
            "Category name cannot be empty.");

        public static readonly Error TooShort = Error.Validation(
            "Catalog.CategoryName.TooShort",
            "Category name must be at least 3 characters long.");

        public static readonly Error TooLong = Error.Validation(
            "Catalog.CategoryName.TooLong",
            "Category name cannot exceed 50 characters.");
    }

    public static class CategoryDescription
    {
        public static readonly Error Empty = Error.Validation(
            "Catalog.CategoryDescription.Empty",
            "Category description cannot be empty.");

        public static readonly Error TooLong = Error.Validation(
            "Catalog.CategoryDescription.TooLong",
            "Category description cannot exceed 500 characters.");
    }
}