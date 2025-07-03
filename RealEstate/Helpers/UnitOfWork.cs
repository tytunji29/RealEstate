using RealEstate.Models;
using RealEstate.Repository;

namespace RealEstate.Helpers;

public interface IUnitOfWork
{
    IPropertyRepository Properties { get; }
    IGenericRepository<PropertyImage> PropertyImages { get; }
    IGenericRepository<EscrowTransaction> EscrowTransactions { get; }
    IGenericRepository<SellerInfo> SellerInfos { get; }
    IGenericRepository<UnverifiedSellerProduct> UnverifiedSellerProducts { get; }
    //IGenericRepository<ViewRecord> ViewRecords { get; }
    IGenericRepository<ApplicationUser> Users { get; }

    Task<int> CompleteAsync();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly RealEstateDbContext _context;

    private readonly IUploadFileService _uploadFileService;
    public IPropertyRepository Properties { get; private set; }
    public IGenericRepository<SellerInfo> SellerInfos { get; }
    public IGenericRepository<UnverifiedSellerProduct> UnverifiedSellerProducts { get; }
    public IGenericRepository<PropertyImage> PropertyImages { get; private set; }
    public IGenericRepository<EscrowTransaction> EscrowTransactions { get; private set; }
    //public IGenericRepository<ViewRecord> ViewRecords { get; private set; }
    public IGenericRepository<ApplicationUser> Users { get; private set; }

    public UnitOfWork(RealEstateDbContext context, IUploadFileService uploadFileService)
    {
        _context = context;
        _uploadFileService = uploadFileService;
        Properties = new PropertyRepository(_context, _uploadFileService);
        PropertyImages = new GenericRepository<PropertyImage>(_context);
        UnverifiedSellerProducts = new GenericRepository<UnverifiedSellerProduct>(_context);
        SellerInfos = new GenericRepository<SellerInfo>(_context);
        EscrowTransactions = new GenericRepository<EscrowTransaction>(_context);
        //ViewRecords = new GenericRepository<ViewRecord>(_context);
        Users = new GenericRepository<ApplicationUser>(_context);
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }
}