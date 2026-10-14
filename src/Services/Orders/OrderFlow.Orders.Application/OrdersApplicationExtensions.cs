using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.BuildingBlocks.Application.Messaging;
using OrderFlow.Inventory.Contracts.Reservations;
using OrderFlow.Orders.Application.Orders.GetOrder;
using OrderFlow.Orders.Application.Orders.PlaceOrder;
using OrderFlow.Orders.Application.Orders.Saga;
using OrderFlow.Payments.Contracts.Orders;

namespace OrderFlow.Orders.Application;

public static class OrdersApplicationExtensions
{
    public static IServiceCollection AddOrdersApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(
            typeof(OrdersApplicationExtensions).Assembly,
            includeInternalTypes: true
        );
        services.AddCommandHandler<PlaceOrderCommand, OrderDto, PlaceOrderCommandHandler>();

        services.AddScoped<IIntegrationMessageHandler<StockReserved>, StockReservedHandler>();
        services.AddScoped<
            IIntegrationMessageHandler<StockReservationFailed>,
            StockReservationFailedHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<OrderPaymentCaptured>,
            OrderPaymentCapturedHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<OrderPaymentFailed>,
            OrderPaymentFailedHandler
        >();
        services.AddScoped<
            IIntegrationMessageHandler<OrderPaymentRefunded>,
            OrderPaymentRefundedHandler
        >();
        services.AddScoped<OrderFulfillment>();

        return services;
    }
}
