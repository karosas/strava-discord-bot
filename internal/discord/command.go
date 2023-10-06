package discord

import "github.com/bwmarrin/discordgo"

type Command interface {
	// Gives most of the power up to the command to decide whether the message should trigger its execution
	// TODO - possibly wrap discord related params into some different struct, IMO commands shouldn't be aware of discord itself
	// since they're going to reside in /cmd/...
	ShouldExecute(m *discordgo.MessageCreate) bool
	Execute(s *discordgo.Session, m *discordgo.MessageCreate)
	GetHelpText() CommandHelpInfo
}

type CommandHelpInfo struct {
	// Without prefix, since that's not accessible here
	// e.g. 'ping'
	TriggerKeyword string
	// Further Description of the command
	// e.g. 'Makes the bot respond with "Pong"'
	Description string
}
