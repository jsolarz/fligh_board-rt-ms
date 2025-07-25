using Microsoft.EntityFrameworkCore;
using FlightBoard.Api.Data;
using FlightBoard.Api.Hubs;
using FlightBoard.Api.Configuration;
using FlightBoard.Api.iFX.Middleware;
using FlightBoard.Api.Services;
using Serilog;

// Configure logging
LoggingConfiguration.ConfigureSerilog();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Configure services
builder.Services.AddDatabaseServices(builder.Configuration, builder.Environment);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCachingServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWebServices(); // This now includes performance optimizations
