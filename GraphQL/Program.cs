using GraphQL.DataAccess;
using GraphQL.GraphQLSchema;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQLServer().AddQueryType<Query>();
builder.Services.AddSingleton<DapperContext>();
var app = builder.Build();

app.MapGraphQL();

app.Run();
