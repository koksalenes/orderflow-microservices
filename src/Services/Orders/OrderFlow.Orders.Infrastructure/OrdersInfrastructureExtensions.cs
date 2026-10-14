using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.BuildingBlocks.Application.Paging;
using OrderFlow.BuildingBlocks.Infrastructure.Messaging;
using OrderFlow.BuildingBlocks.Infrastructure.Persistence;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Application;
using OrderFlow.Orders.Application.Orders;
using OrderFlow.Orders.Application.Orders.GetMyOrders;
using OrderFlow.Orders.Application.Orders.GetOrder;
using OrderFlow.Orders.Infrastructure.Catalog;
using OrderFlow.Orders.Infrastructure.Fulfillment;
using OrderFlow.Orders.Infrastructure.Orders;
using OrderFlow.Orders.Infrastructure.Orders.GetMyOrders;
using OrderFlow.Orders.Infrastructure.Orders.GetOrder;
using OrderFlow.Orders.Infrastructure.Persistence;
using OrderFlow.Payments.Contracts.Orders;
using OrderFlow.Products.Contracts.Grpc;

namespace OrderFlow.Orders.Infrastructure;

public static class OrdersInfrastructureExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services)
    {
        services.AddOrdersApplication();
        services.AddPostgresDbContext<OrdersDbContext>();
        services.AddUnitOfWork<OrdersDbContext>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();

        services
            .AddOptions<FulfillmentOptions>()
            .BindConfiguration(FulfillmentOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<FulfillmentOptions>>().Value;
            return new FulfillmentSchedule(
                TimeSpan.FromSeconds(options.ConfirmAfterSeconds),
                TimeSpan.FromSeconds(options.ShipAfterSeconds),
                TimeSpan.FromSeconds(options.DeliverAfterSeconds),
                TimeSpan.FromSeconds(options.InvoiceAfterSeconds)
            );
        });

        services.AddQueryHandler<GetOrderQuery, OrderDto, GetOrderQueryHandler>();
        services.AddQueryHandler<
            GetMyOrdersQuery,
            PagedResult<OrderSummaryDto>,
            GetMyOrdersQueryHandler
        >();

        return services;
    }

    public static IServiceCollection AddOrdersCatalog(this IServiceCollection services)
    {
        services
            .AddOptions<ServicesOptions>()
            .BindConfiguration(ServicesOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddGrpcClient<ProductCatalog.ProductCatalogClient>(
            (provider, client) =>
                client.Address = provider
                    .GetRequiredService<IOptions<ServicesOptions>>()
                    .Value.ProductsGrpc
        );
        services.AddScoped<IProductCatalog, GrpcProductCatalog>();

        return services;
    }

    public static IServiceCollection AddOrdersMessaging(this IServiceCollection services)
    {
        services.AddOutboxPublisher();
        services.AddIntegrationConsumer<StockReserved>(
            StockReserved.MessageType,
            "orders.order_saga.stock_reserved"
        );
        services.AddIntegrationConsumer<StockReservationFailed>(
            StockReservationFailed.MessageType,
            "orders.order_saga.stock_reservation_failed"
        );
        services.AddIntegrationConsumer<OrderPaymentCaptured>(
            OrderPaymentCaptured.MessageType,
            "orders.order_saga.order_payment_captured"
        );
        services.AddIntegrationConsumer<OrderPaymentFailed>(
            OrderPaymentFailed.MessageType,
            "orders.order_saga.order_payment_failed"
        );
        services.AddIntegrationConsumer<OrderPaymentRefunded>(
            OrderPaymentRefunded.MessageType,
            "orders.order_saga.order_payment_refunded"
        );
        services.AddHostedService<FulfillmentScheduler>();

        return services;
    }

    public static Task MigrateOrdersDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.MigrateDatabaseAsync<OrdersDbContext>(cancellationToken);

    public static Task WaitForOrdersDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    ) => services.WaitForMigrationsAsync<OrdersDbContext>(cancellationToken);
}
