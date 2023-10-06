package commands

import (
	"github.com/bwmarrin/discordgo"
	discord "github.com/karosas/strava-discord-bot/internal/discord"
)

type SamplePingPongCommand struct {
}

func (c *SamplePingPongCommand) ShouldExecute(m *discordgo.MessageCreate) bool {
	return m.Content == "ping"
}

func (c *SamplePingPongCommand) Execute(s *discordgo.Session, m *discordgo.MessageCreate) {
	s.ChannelMessageSend(m.ChannelID, "pong")
}

func (c *SamplePingPongCommand) GetHelpText() discord.CommandHelpInfo {
	return discord.CommandHelpInfo{TriggerKeyword: "ping", Description: "Makes bot respond with 'Pong'"}
}
