const std = @import("std");
const Allocator = std.mem.Allocator;

pub fn getAlloc(args: struct { allocator: Allocator, url: []const u8, mimeType: []const u8 = "application/json", cookieHeader: ?std.http.Header = null }) ![]u8 {
    const serverHeaderBuffer = try args.allocator.alloc(u8, 1024 * 4);
    defer args.allocator.free(serverHeaderBuffer);

    var client = std.http.Client{ .allocator = args.allocator };
    defer client.deinit();

    const uri = try std.Uri.parse(args.url);

    var request: std.http.Client.Request = undefined;

    if (args.cookieHeader) |cookie| {
        request = try client.open(.GET, uri, .{ .server_header_buffer = serverHeaderBuffer, .extra_headers = &.{cookie} });
    } else {
        request = try client.open(.GET, uri, .{ .server_header_buffer = serverHeaderBuffer });
    }

    defer request.deinit();

    request.headers.accept_encoding = .{ .override = args.mimeType };

    try request.send();
    try request.finish();
    try request.wait();

    try std.testing.expectEqual(.ok, request.response.status);

    const reader = request.reader();
    var bufferedReader = std.io.bufferedReader(reader);

    const body = try bufferedReader.reader().readAllAlloc(args.allocator, 24 * 1024);

    return body;
}

test "navigate to page" {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer std.debug.assert(gpa.deinit() == .ok);

    const allocator = gpa.allocator();

    const response = try getAlloc(.{ .allocator = allocator, .url = "https://jsonplaceholder.typicode.com/todos/1" });
    defer allocator.free(response);

    //potentially flakey test - unsure if this is static text but its been static in testing so far
    try std.testing.expectEqualStrings(
        \\{
        \\  "userId": 1,
        \\  "id": 1,
        \\  "title": "delectus aut autem",
        \\  "completed": false
        \\}
    , response);
}
