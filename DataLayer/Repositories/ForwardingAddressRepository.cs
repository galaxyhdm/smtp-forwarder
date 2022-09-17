﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmtpForwarder.Application.Interfaces.Repositories;
using SmtpForwarder.Domain;

namespace SmtpForwarder.DataLayer.Repositories;

internal sealed class ForwardingAddressRepository : RootRepositoryBase<ForwardingAddress>, IForwardingAddressRepository
{

    public ForwardingAddressRepository(AppDbContext context, ILogger<ForwardingAddressRepository> logger) : base(context)
    {
        Entities = context.Set<ForwardingAddress>()
            .Include(f => f.Owner)
            .Include(f => f.ForwardTarget)
            .AsQueryable();

        BeforeAdd += (_, args) => args.Entity.LogCreatedUpdated(true);
        BeforeUpdate += (_, args) => args.Entity.LogCreatedUpdated();
        BeforeSave += (_, args) => logger.LogDebug("Saving forwarding_address {} - {}",
            args.Entity.Id,
            args.Entity.LocalAddressPart);
    }
}