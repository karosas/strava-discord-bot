package discord

import (
	"fmt"
	"log/slog"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"github.com/bwmarrin/discordgo"
)

type DiscordClient struct {
	commands      []Command
	token         string
	commandPrefix string
}

func NewDiscordClient(token string, commands []Command, commandPrefix string) *DiscordClient {
	client := &DiscordClient{
		commands:      commands,
		token:         token,
		commandPrefix: commandPrefix,
	}

	return client
}

func (c *DiscordClient) Run() {
	dg, err := discordgo.New("Bot " + c.token)
	if err != nil {
		slog.Error("Error creating Discord session, ", err)
	}

	dg.AddHandler(c.messageCreate)

	dg.Identify.Intents = discordgo.IntentsGuildMessages | discordgo.IntentsMessageContent

	err = dg.Open()
	if err != nil {
		slog.Error("error opening connection,", err)
		return
	}

	fmt.Println("Bot is running. CTRL-C to stop.")
	sc := make(chan os.Signal, 1)
	signal.Notify(sc, syscall.SIGINT, syscall.SIGTERM, os.Interrupt)
	<-sc

	dg.Close()
}

func (c *DiscordClient) messageCreate(s *discordgo.Session, m *discordgo.MessageCreate) {
	if m.Author.ID == s.State.User.ID {
		return
	}

	if !strings.HasPrefix(m.Content, c.commandPrefix) {
		return
	}

	prefixlessContent := removePrefixAndWhiteSpace(m.Content, c.commandPrefix)

	if prefixlessContent == "help" {
		s.ChannelMessageSend(m.ChannelID, generateHelpMessage(c.commandPrefix, c.commands))
		return
	}

	for _, command := range c.commands {
		if command.ShouldExecute(m) {
			command.Execute(s, m)
		}
	}
}

func removePrefixAndWhiteSpace(input string, prefix string) string {
	prefixless := strings.Replace(input, prefix, "", 1)
	return strings.ToLower(strings.Trim(prefixless, " \t\n\r"))
}

func generateHelpMessage(prefix string, commands []Command) string {
	var sb strings.Builder

	sb.WriteString("Strava Leaderboard Discord Bot Usage:\n")
	sb.WriteString("`" + prefix + " {command}`, commands: \n")

	for _, command := range commands {
		cmdHelpInfo := command.GetHelpText()
		sb.WriteString("  - `" + cmdHelpInfo.TriggerKeyword + "` - " + cmdHelpInfo.Description + "\n")
	}

	return sb.String()
}
