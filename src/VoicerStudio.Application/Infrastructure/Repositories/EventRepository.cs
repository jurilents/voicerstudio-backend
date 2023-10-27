// using System.Text.Json;
// using Microsoft.Extensions.Options;
// using MongoDB.Bson;
// using MongoDB.Driver;
// using NeerCore.Exceptions;
// using VoicerStudio.Application.Options;
// using VoicerStudio.Application.Repositories;
//
// namespace VoicerStudio.Application.Infrastructure.Repositories;
//
// internal class EventRepository : IEventRepository
// {
//     private readonly MongoOptions _options;
//
//     public EventRepository(IOptions<MongoOptions> optionsAccessor)
//     {
//         _options = optionsAccessor.Value;
//     }
//
//
//     public async Task<object> GetTimingAsync(string slug, CancellationToken ct = default)
//     {
//         var db = GetDatabase();
//         var collection = db.GetCollection<BsonDocument>("events");
//
//         var @event = await collection.Find(
//                 Builders<BsonDocument>.Filter.Eq("slug", slug))
//             .FirstOrDefaultAsync(ct);
//         if (@event is null)
//             throw new NotFoundException("Event not found");
//
//         return JsonSerializer.Deserialize<object>(@event["timings"].AsString)!;
//     }
//
//     public async Task SetTimingAsync(string slug, object timing, CancellationToken ct = default)
//     {
//         var timingJsonString = JsonSerializer.Serialize(timing);
//
//         var db = GetDatabase();
//         var collection = db.GetCollection<BsonDocument>("events");
//
//         var existedCount = await collection.Find(
//                 Builders<BsonDocument>.Filter.Eq("slug", slug))
//             .CountDocumentsAsync(ct);
//
//         if (existedCount > 0)
//         {
//             // Update
//             var filter = Builders<BsonDocument>.Filter.Eq("slug", slug);
//             var update = Builders<BsonDocument>.Update.Set("timings", timingJsonString);
//             await collection.UpdateOneAsync(filter, update, cancellationToken: ct);
//         }
//         else
//         {
//             // Create
//             await collection.InsertOneAsync(new BsonDocument(new Dictionary<string, object>
//             {
//                 ["slug"] = slug,
//                 ["timings"] = timingJsonString,
//                 ["timestamp"] = DateTime.UtcNow,
//             }), cancellationToken: ct);
//         }
//     }
//
//
//     private IMongoDatabase GetDatabase()
//     {
//         var settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
//         settings.ServerApi = new ServerApi(ServerApiVersion.V1);
//         var mongoClient = new MongoClient(settings);
//         return mongoClient.GetDatabase(_options.DatabaseName);
//     }
// }