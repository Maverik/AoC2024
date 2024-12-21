// SPDX-License-Identifier: CC-BY-NC-SA-4.0
// SPDX-FileCopyrightText: ©️ 2024 Maverik <http://github.com/Maverik>

const std = @import("std");
const Allocator = std.mem.Allocator;
const httpClient = @import("./httpClient.zig");

pub fn part1() !void {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer std.debug.assert(gpa.deinit() == .ok);

    const allocator = gpa.allocator();

    //Must define an environment variable with the full cookie value from AoC post login (so it should have both ru & session values in it)
    //The environment variable muct be named "AoC2004-FullCookie"
    const cookieHeaderValue = std.process.getEnvVarOwned(allocator, "AoC2024-FullCookie") catch |err| switch (err) {
        error.EnvironmentVariableNotFound => @panic("Cannot continue without environment variable AoC2004-FullCookie. This should be the full cookie value from AoC authenticated session include ru & session values"),
        error.OutOfMemory, error.InvalidWtf8 => @panic("This was not supposed to happen..."),
    };

    defer allocator.free(cookieHeaderValue);

    const response = try httpClient.getAlloc(.{ .allocator = allocator, .url = "https://adventofcode.com/2024/day/1/input", .cookieHeader = .{ .name = "Cookie", .value = cookieHeaderValue } });
    defer allocator.free(response);

    const stdoutWriter = std.io.getStdOut();
    var bufferedWriter = std.io.bufferedWriter(stdoutWriter.writer());
    const w = bufferedWriter.writer();

    try w.print("Distance: {d}", .{try parseInput(allocator, response)});

    try bufferedWriter.flush();
}

fn parseInput(allocator: Allocator, input: []const u8) !u32 {
    var numbers = std.mem.tokenizeAny(u8, input, " \n");

    //buffer is one character per element and there's 2 numbers 5 character long each with 3 spaces and 1 newline: 2x5+3+1
    const lineCount = numbers.buffer.len / 14;

    var leftList = try std.ArrayList(u32).initCapacity(allocator, lineCount);
    defer leftList.deinit();

    var rightList = try std.ArrayList(u32).initCapacity(allocator, lineCount);
    defer rightList.deinit();

    while (numbers.next()) |left| {
        try leftList.append(try parseBufferToU32(left));
        try rightList.append(try parseBufferToU32(numbers.next().?));
    }

    const lefts = leftList.items;
    const rights = rightList.items;

    std.mem.sort(u32, lefts, {}, std.sort.asc(u32));
    std.mem.sort(u32, rights, {}, std.sort.asc(u32));

    var accumulator: u32 = 0;

    for (lefts, rights) |left, right| {
        if (left >= right) {
            accumulator += left - right;
        } else {
            accumulator += right - left;
        }
    }

    return accumulator;
}

fn parseBufferToU32(buffer: []const u8) !u32 {
    return try std.fmt.parseUnsigned(u32, buffer, 10);
}

test "parse basic input correctly for one line" {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer std.debug.assert(gpa.deinit() == .ok);

    const allocator = gpa.allocator();

    const buffer = "3 7";
    try std.testing.expectEqual(4, try parseInput(allocator, buffer));
}

test "parse basic input correctly for multi line" {
    var gpa = std.heap.GeneralPurposeAllocator(.{}){};
    defer std.debug.assert(gpa.deinit() == .ok);

    const allocator = gpa.allocator();

    const buffer = "3 7\n7 3\n0 0";
    try std.testing.expectEqual(0, try parseInput(allocator, buffer));
}
