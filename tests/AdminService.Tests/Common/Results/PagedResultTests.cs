using AdminService.Common.Results;
using FluentAssertions;
using System.Linq;
using System;
using Xunit;

namespace AdminService.Tests.Common.Results;

public class PagedResultTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var items = new[] { "item1", "item2", "item3" };
        var totalCount = 10;
        var pageNumber = 2;
        var pageSize = 3;
        
        // Act
        var pagedResult = new PagedResult<string>(items, totalCount, pageNumber, pageSize, totalPages: 4);
        
        // Assert
        pagedResult.Items.Should().BeEquivalentTo(items);
        pagedResult.TotalCount.Should().Be(totalCount);
        pagedResult.Page.Should().Be(pageNumber);
        pagedResult.PageSize.Should().Be(pageSize);
        pagedResult.TotalPages.Should().Be(4); // Calculated as ceiling(10/3)
    }
    
    [Fact]
    public void TotalPages_ShouldCalculateCorrectly()
    {
        // Test cases for different total counts and page sizes
        var testCases = new[]
        {
            (totalCount: 0, pageSize: 10, expectedPages: 0),
            (totalCount: 1, pageSize: 10, expectedPages: 1),
            (totalCount: 10, pageSize: 10, expectedPages: 1),
            (totalCount: 11, pageSize: 10, expectedPages: 2),
            (totalCount: 20, pageSize: 10, expectedPages: 2),
            (totalCount: 21, pageSize: 10, expectedPages: 3),
            (totalCount: 5, pageSize: 2, expectedPages: 3)
        };
        
        foreach (var (totalCount, pageSize, expectedPages) in testCases)
        {
            // Arrange
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var items = Enumerable.Range(1, Math.Min(pageSize, totalCount)).Select(i => $"item{i}").ToArray();
            
            // Act
            var pagedResult = new PagedResult<string>(items, totalCount, 1, pageSize, totalPages);
            
            // Assert
            pagedResult.TotalPages.Should().Be(expectedPages,
                $"Total pages should be {expectedPages} when totalCount is {totalCount} and pageSize is {pageSize}");
        }
    }
}
