using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace URBANFORT.Data;
public class ItemDatabase : DbContext
{
    public DbSet<Collection> Collections { get; set; }
    public DbSet<Item> Items { get; set; }

    public string Path { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={Path}");
    }

    public ItemDatabase()
    {
    }

    public void Initialize ()
    {
        Database.EnsureCreated();
    }
}

public class Collection
{
    public int CollectionId { get; set; }
    public string Name { get; set; }
    public ICollection<CollectionItem> CollectionItems { get; set; }
}

public class CollectionItem
{
    public int CollectionItemId { get; set; }
    // Composite key
    public int CollectionId { get; set; }
    // Composite key
    public int ItemId { get; set; }
    // Reference to the collection
    public Collection Collection { get; set; }
    // Reference to the item
    public Item Item { get; set; }
    // The distribution of the item in the collection, how likely it is to be found in rolls
    public float Distribution { get; set; }
    // The price of the item in the collection
    public int Price { get; set; }
}

public class Item
{
    // Primary key
    public int ItemId { get; set; }
    // A unqiue-ish name
    public string Name { get; set; }
    // The base price
    public int Price { get; set; }

    [NotMapped]
    public object Resource { get; set; }
}