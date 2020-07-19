# Strava Leaderboard Bot for Discord

## Introduction

Made to use in a specific community server, but designed to serve multiple servers with participants being isolated between servers (leaderboards).

At the moment I'm not showing how to add the bot to your server (instance hosted by me), because I'm not sure yet how much load a single droplet instance can handle, but you're welcome to self-host it.

## Customizing leaderboard

After recent rework I attempted to redesign the leaderboard so it would be much easier to create/modify leaderboard's categories/subcategories.

## Adding categories (e.g. Real Ride, Virtual Ride, Running, etc.)

1. Create a new class implementing `ICategory` ( I think I documented it well enough on how it needs to be implemented and the interface is simple enough).
2. Add an instance of a new category to wherever we call `ILeaderboardService.GenerateForServer` (e.g. in `WeeklyLeaderboardHostedService` to change the automated leaderboard)

## Adding sub-categories (e.g. Distance, Power, Elevation, etc.)

1. Create a new class implementing `ISubCategory`
2. Add an instance of this new subcategory to desired existing category's `SubCategories` property.

**Note -** Keep in mind that that currently we're generating 1 embed per category and all display data (titles, winners, etc.) are added as embed fields. This is a slight problem, because Discord has a limit of 25 fields. 1 sub-category adds 4 fields (title and top 3 winners). At the moment categories have 4 sub categories, so we're already using 17 fields (4 sub-categories (16 fields) + 1 fields for title with category, date).

Other suggestions are welcome as well.
