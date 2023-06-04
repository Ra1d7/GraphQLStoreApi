using GraphQL.DataAccess;
using GraphQL.GraphQLSchema;
using GraphQL.Mutations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQLServer()
    .AddMutationType<Mutation>()
    .AddQueryType<Query>();
builder.Services.AddSingleton<DapperContext>();
var app = builder.Build();

app.MapGraphQL();

app.Run();
