## Implementing the Packing List Aggregate

In this chapter, we will introduce and implement the `PackingList` aggregate within our domain model. We'll start by focusing on the domain layer, where we'll define the aggregate root, its entities, and value objects. Following that, we'll implement the necessary infrastructure components, such as data models and repositories. 

In later chapters, we'll move on to the application layer, where we'll define services and DTOs. Finally, we'll tie everything together in the web layer, where users can interact with the system via controllers and views.
## Domain Layer: Defining the PackingList Aggregate

The domain layer is where the core business logic resides. In Domain-Driven Design (DDD), an aggregate is a cluster of domain objects that can be treated as a single unit. The `PackingList` aggregate will contain the `PackingList` as the aggregate root and `Item` as an entity within that aggregate. 

The aggregate is managed by an **aggregate root**, which is the only entity within the aggregate that is accessible from outside. The aggregate root ensures the consistency of changes across the aggregate by enforcing business rules and invariants
### Directory Structure

To organize our domain model, we'll create a subfolder called `Aggregates` within the domain layer. Inside `Aggregates`, we'll create a folder for the `PackingList` aggregate.

```plaintext
src/
├── PackAndGo.Domain/
│   └── Aggregates/
│       └── PackingList/
│           ├── Entities/
|           |   |──Item.cs
|           |   └──PackingList.cs
│           └── ValueObjects/
```

###  Implementing the PackingList Aggregate Root

The `PackingList` will serve as the aggregate root. It contains a collection of `Item` entities and manages the business logic related to packing lists. But first we implement the Item entity

The `Item` entity is part of the `PackingList` aggregate and is managed by the `PackingList` aggregate root.

> `Item.cs`

```csharp
using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

public class Item : Entity
{
    public string Name { get; private set; }
    public bool IsPacked { get; private set; }

    private Item(Guid id, string name, bool isPacked)
    {
        Id = id;
        Name = name;
        IsPacked = isPacked;
    }

    public static Item Create(string name)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        return new Item(Guid.NewGuid(), name, false);
    }

    public static Item Load(Guid id, string name, bool isPacked)
    {
        var item = Item.Create(name);
        item.Id = id;
        item.IsPacked = isPacked;
        return item;
    }

    public void ChangeName(string name)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        Name = name;
    }
    
    public void MarkAsPacked()
    {
        IsPacked = true;
    }

    public void MarkAsUnpacked()
    {
        IsPacked = false;
    }
}
```

**Value Objects**

If needed, we can create value objects within the `ValueObjects` folder under the `PackingList` aggregate. For now, we keep it with primitive datatypes.

> Doing this is sometimes referred to as _Primitive Obsession_ and is one of the _Code Smells_ for an area of improvement. In DDD you rather have specific datatypes that upholds their internal integrity.

.

> `PackingList.cs`

```csharp
using PackAndGo.Domain.Common;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

public class PackingList : Entity
{
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public ICollection<Item> Items { get; private set; }

    private PackingList(Guid id, string name, Guid ownerId, IEnumerable<Item> items)
    {
        Id = id;
        Name = name;
        OwnerId = ownerId;
        Items = items.ToList();
    }

    public static PackingList Create(string name, Guid ownerId)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }
        // Validate ownerId
        if (ownerId == Guid.Empty)
        {
            throw new ArgumentNullException(nameof(ownerId));
        }
        return new PackingList(Guid.NewGuid(), name, ownerId, new List<Item>());
    }

    public static PackingList Load(Guid id, string name, Guid ownerId, IEnumerable<Item> items)
    {
        var packingList = new PackingList(id, name, ownerId, items);
        return packingList;
    }

    public void ChangeName(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void AddItem(string itemName)
    {
        var item = Item.Create(itemName);
        Items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void ChangeItemName(Guid itemId, string name)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.ChangeName(name);
    }

    public void MarkItemAsPacked(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.MarkAsPacked();
    }

    public void MarkItemAsUnpacked(Guid itemId)
    {
        var item = Items.SingleOrDefault(i => i.Id == itemId);
        item?.MarkAsUnpacked();
    }
}
```

**Repository Interface**

In Domain-Driven Design (DDD), the repository pattern is used to abstract the persistence logic and provide a clean interface for accessing and manipulating aggregates. By defining a repository interface in the domain layer, we ensure that the domain logic remains independent of the underlying data access implementation, promoting a clear separation of concerns.

The `IPackingListRepository` interface defines the contract for working with the `PackingList` aggregate. It includes methods for retrieving, adding, updating, and deleting `PackingList` instances. This interface will be implemented in the infrastructure layer, where the actual data access logic resides.

> `IPackingListRepository.cs`

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Domain.Aggregates.PackingListAggregate.Repositories;

public interface IPackingListRepository
{
    Task<PackingList?> GetByIdAsync(Guid id);
    Task<IEnumerable<PackingList>> GetAllByOwnerIdAsync(Guid ownerId);
    Task AddAsync(PackingList packingList);
    Task UpdateAsync(PackingList packingList);
    Task DeleteAsync(Guid id);
}
```

**Explanation**

- **`GetByIdAsync(Guid id)`**: Retrieves a `PackingList` by its unique identifier. This method returns the aggregate root along with its related entities (`Item`s), or `null` if the `PackingList` is not found.
- **`GetByOwnerIdAsync(Guid ownerId)`**: This method retrieves all PackingList instances that are owned by a specific user, identified by the ownerId parameter. It returns a collection of PackingList objects that belong to the specified user.
- **`AddAsync(PackingList packingList)`**: Adds a new `PackingList` to the repository. This method is used when creating a new packing list.
- **`UpdateAsync(PackingList packingList)`**: Updates an existing `PackingList` in the repository. This method is called when modifications are made to the packing list or its items.
- **`DeleteAsync(Guid id)`**: Deletes a `PackingList` from the repository by its unique identifier.

By defining this repository interface in the domain layer, we adhere to the DDD principle of keeping the domain model free from infrastructure concerns, ensuring that our business logic remains focused and adaptable to changes in the underlying persistence mechanism.
















## Infrastructure Layer: Persistence and Data Access

The infrastructure layer is responsible for data persistence and access. We'll implement the data models, a repository, and a DbContext to manage `PackingList` and `Item` entities.

### Directory Structure

We'll create a corresponding folder structure in the infrastructure layer.

```plaintext
src/
├── PackAndGo.Infrastructure/
│   └── Repositories/
│       └── PackingListRepository.cs
│   └── DataModels/
│       ├── PackingListDataModel.cs
│       └── ItemDataModel.cs
│   └── DbContexts/
│       └── PackAndGoDbContext.cs
```

### Data Models

Data models represent how the entities are stored in the database. We'll create separate data models for `PackingList` and `Item`.

> `ItemDataModel.cs`

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class ItemDataModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsPacked { get; set; }

    // Mapping method to convert ItemDataModel to domain Item
    public Item ToDomain()
    {
        var item = Item.Load(Id, Name ?? string.Empty, IsPacked); // Load the item with the Id and Name
        return item;
    }

    // Mapping method to convert domain Item to ItemDataModel
    public static ItemDataModel FromDomain(Item item)
    {
        return new ItemDataModel
        {
            Id = item.Id,
            Name = item.Name,
            IsPacked = item.IsPacked
        };
    }
}

```

> `PackingListDataModel.cs`

```csharp
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;

namespace PackAndGo.Infrastructure.DataModels;

public class PackingListDataModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid UserId { get; set; }
    public ICollection<ItemDataModel>? Items { get; set; }

    // Mapping method to convert PackingListDataModel to domain PackingList
    public PackingList ToDomain()
    {
        // Convert each ItemDataModel to Item domain entity
        var items = Items?.Select(i => i.ToDomain()).ToList() ?? new List<Item>();

        // Create the PackingList domain entity
        var packingList = PackingList.Load(
            Id,
            Name ?? string.Empty,  // Ensure Name is non-null
            UserId,
            items
        );

        return packingList;
    }

    // Mapping method to convert domain PackingList to PackingListDataModel
    public static PackingListDataModel FromDomain(PackingList packingList)
    {
        // Convert each Item domain entity to ItemDataModel
        var itemDataModels = packingList.Items.Select(ItemDataModel.FromDomain).ToList();

        return new PackingListDataModel
        {
            Id = packingList.Id,
            Name = packingList.Name,
            UserId = packingList.OwnerId,
            Items = itemDataModels
        };
    }
}
```

### PackingList Repository

The repository pattern is used to abstract the data access logic. We'll create a repository for `PackingList` that handles CRUD operations.

> `PackingListRepository.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Entities;
using PackAndGo.Domain.Aggregates.PackingListAggregate.Repositories;
using PackAndGo.Infrastructure.DataModels;
using PackAndGo.Infrastructure.Persistence;

namespace PackAndGo.Infrastructure.Repositories;

public class PackingListRepository : IPackingListRepository
{
    private readonly AppDbContext _context;

    public PackingListRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PackingList?> GetByIdAsync(Guid id)
    {
        var packingListDataModel = await _context.PackingLists
            .Include(pl => pl.Items)
            .SingleOrDefaultAsync(pl => pl.Id == id);

        return packingListDataModel?.ToDomain();
    }

    public async Task<IEnumerable<PackingList>> GetAllByOwnerIdAsync(Guid ownerId)
    {
        var packingListDataModels = await _context.PackingLists
            .Include(pl => pl.Items)
            .Where(pl => pl.UserId == ownerId)
            .ToListAsync();

        return packingListDataModels.Select(pl => pl.ToDomain()).ToList();
    }

    public async Task AddAsync(PackingList packingList)
    {
        var packingListDataModel = PackingListDataModel.FromDomain(packingList);
        _context.PackingLists.Add(packingListDataModel);
        await _context.SaveChangesAsync();
    }

public async Task UpdateAsync(PackingList packingList)
{
    var existingPackingList = await _context.PackingLists
        .Include(pl => pl.Items)
        .SingleOrDefaultAsync(pl => pl.Id == packingList.Id);

    if (existingPackingList != null)
    {
        // Update the existing PackingListDataModel
        existingPackingList.Name = packingList.Name;
        existingPackingList.UserId = packingList.OwnerId;

        // Remove items that are no longer in the updated list
        foreach (var existingItem in existingPackingList.Items?.ToList() ?? new List<ItemDataModel>())
        {
            if (!packingList.Items.Any(i => i.Id == existingItem.Id))
            {
                _context.Items.Remove(existingItem);
            }
        }

        // Add or update the items in the existing list
        foreach (var item in packingList.Items)
        {
            var existingItem = existingPackingList.Items?.SingleOrDefault(i => i.Id == item.Id);
            if (existingItem == null)
            {
                // Ensure EF Core tracks the new item correctly
                var newItemDataModel = ItemDataModel.FromDomain(item);
                _context.Entry(newItemDataModel).State = EntityState.Added;
                existingPackingList.Items?.Add(newItemDataModel);
            }
            else
            {
                existingItem.Name = item.Name;
                existingItem.IsPacked = item.IsPacked;
                _context.Entry(existingItem).State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync();
    }
}

    public async Task DeleteAsync(Guid id)
    {
        var packingListDataModel = await _context.PackingLists
            .Include(pl => pl.Items)
            .SingleOrDefaultAsync(pl => pl.Id == id);

        if (packingListDataModel != null)
        {
            _context.PackingLists.Remove(packingListDataModel);
            await _context.SaveChangesAsync();
        }
    }
}
```

**Explanation**

1. **GetByIdAsync(Guid id)**:
   - This method retrieves a `PackingList` by its unique identifier. It includes the related `Items` and maps the data model to the domain entity using the `MapToDomainEntity` method.

2. **GetAllByOwnerIdAsync(Guid ownerId)**:
   - This method retrieves all `PackingList` instances owned by a specific user. It queries the database using the `OwnerId` and returns a list of domain entities.

3. **AddAsync(PackingList packingList)**:
   - This method adds a new `PackingList` to the database. It maps the domain entity to a data model and persists it using Entity Framework Core.

4. **UpdateAsync(PackingList packingList)**:
   - This method updates an existing `PackingList` in the database. It first retrieves the existing data model, then updates it with the new data from the domain entity.

5. **DeleteAsync(Guid id)**:
   - This method deletes a `PackingList` from the database by its unique identifier.

This implementation of the `PackingListRepository` is fully aligned with the `IPackingListRepository` interface, handling the persistence of `PackingList` and `Item` entities while ensuring the business logic in the domain layer remains decoupled from the infrastructure concerns.


### DbContext Implementation

Let's develop the `DbContext` for the `PackAndGo` application based on the provided data models. The `DbContext` class is responsible for managing the database connection and handling the configuration of the entity models, such as `PackingListDataModel` and `ItemDataModel`.

> `PackAndGoDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using PackAndGo.Infrastructure.DataModels;

namespace PackAndGo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
   public DbSet<UserDataModel> Users { get; set; }
    public DbSet<PackingListDataModel> PackingLists { get; set; }
    public DbSet<ItemDataModel> Items { get; set; }

   public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
       modelBuilder.Entity<UserDataModel>().HasData(
           new UserDataModel { Id = Guid.NewGuid(), Email = "john.doe@test.com" },
           new UserDataModel { Id = Guid.NewGuid(), Email = "jane.doe@test.com" },
           new UserDataModel { Id = Guid.NewGuid(), Email = "jacob.doe@test.com" }
       );
   }
}
```

**Explanation**

1. **DbSet Properties**:
   - `PackingLists`: Represents the `PackingListDataModel` entity set in the database.
   - `Items`: Represents the `ItemDataModel` entity set in the database.

### Update the database

**Database Relationships**

- **PackingList to Items (One-to-Many)**:
  - A `PackingListDataModel` can have many `ItemDataModel` instances.
  - Each `ItemDataModel` belongs to exactly one `PackingListDataModel`.
  - This relationship is enforced via a foreign key (`PackingListId`), and cascade delete behavior ensures that deleting a `PackingList` also deletes its associated `Items`.

In order to update the database according to our new implementation, let us do a migration:

```bash
dotnet ef migrations add AddPackingLists --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
dotnet ef database update --project src/PackAndGo.Infrastructure --startup-project src/PackAndGo.Web
```

### **Learning Outcomes**

By the end of this chapter, you should have a solid understanding of how to define and implement a domain aggregate, specifically the `PackingList` aggregate, and how to use it within a layered architecture. Below are the key learning outcomes:

1. **Understanding Aggregates in Domain-Driven Design (DDD)**:
   - You learned that an **aggregate** is a collection of related domain entities treated as a single unit, with an aggregate root (in this case, the `PackingList`) managing consistency and ensuring the validity of operations within the aggregate.
   - The **aggregate root** (`PackingList`) enforces business rules and encapsulates domain logic, while related entities (such as `Item`) are managed internally and are inaccessible from outside the aggregate.

2. **Implementing Entities and Aggregates**:
   - You defined both the `PackingList` and `Item` entities within the domain model.
   - The `PackingList` serves as the aggregate root and manages the collection of `Item` entities, with business logic for adding, removing, and modifying `Items`.

3. **Handling Domain Logic**:
   - You implemented core domain logic in the `PackingList` and `Item` entities, including validation for inputs (like names) and business operations such as marking an item as packed or unpacked.
   - You learned the importance of **invariants** and **business rule validation**, which were embedded into the aggregate and its entities (e.g., ensuring an item's name is not empty).

4. **Repository Pattern in DDD**:
   - You explored the **repository pattern**, which abstracts persistence logic and provides a clean interface for accessing domain entities. You saw how repositories such as `IPackingListRepository` ensure that domain logic is decoupled from infrastructure concerns.
   - The repository pattern also promotes testability and flexibility, making it easier to swap out data access mechanisms without affecting the domain layer.

5. **Mapping Between Data Models and Domain Entities**:
   - You learned how to convert between **data models** (used in the infrastructure layer) and **domain entities** (used in the domain layer). This mapping ensures that domain entities remain free of persistence concerns, even when interacting with a database.
   - The mapping methods (`ToDomain` and `FromDomain`) were used to bridge the gap between the domain model and the persistence layer.

6. **Implementing Infrastructure Components**:
   - You implemented the **infrastructure layer** for the `PackingList` aggregate, including data models, repositories, and the `DbContext` for managing persistence with Entity Framework Core.
   - You learned how the **data models** (`PackingListDataModel` and `ItemDataModel`) mirror the domain model but are used specifically for interacting with the database.

7. **Understanding One-to-Many Relationships**:
   - You explored the relationship between `PackingList` and `Item` as a **one-to-many** relationship, and how Entity Framework Core manages such relationships via foreign keys.
   - You also learned how cascade delete behavior can be enforced in such relationships, ensuring that when a `PackingList` is deleted, its associated `Items` are also removed from the database.

8. **Database Migration with Entity Framework Core**:
   - You saw how to create and apply a **database migration** using Entity Framework Core to reflect the changes made to the domain model and ensure the database schema stays in sync with your code.

By applying these concepts, you now have a clear understanding of how to implement domain aggregates and their related infrastructure components in a DDD-based project. You've also learned how to effectively manage persistence using the repository pattern and how to maintain a clean separation of concerns between the domain and infrastructure layers.

