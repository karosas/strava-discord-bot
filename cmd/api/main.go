package main

import (
	"os"

	commands "github.com/karosas/strava-discord-bot/cmd/api/commands"
	config "github.com/karosas/strava-discord-bot/internal"
	discord "github.com/karosas/strava-discord-bot/internal/discord"
)

var (
	registeredCommands []discord.Command
)

func init() {
	config.Load()
	registeredCommands = []discord.Command{
		new(commands.SamplePingPongCommand),
	}
}

func main() {
	client := discord.NewDiscordClient(os.Getenv("DISCORD_TOKEN"), registeredCommands, os.Getenv("DISCORD_PREFIX"))
	client.Run()
}
