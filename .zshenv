# tmux
export TERM="xterm-256color"

# bspwm
export PANEL_FIFO="/tmp/panel-fifo"
export XDG_CONFIG_HOME="$HOME/.config"

# oh-my-zsh
export ZSH=$HOME/.config/oh-my-zsh
export DEFAULT_USER="zorbik"

# powerlevel9k configuration
export POWERLEVEL9K_LEFT_PROMPT_ELEMENTS=(dir rbenv vcs)
export POWERLEVEL9K_RIGHT_PROMPT_ELEMENTS=(status root_indicator background_jobs history time)

# dotnet
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# prison architect sound fix
export SND_AUDIOSYSTEM=pulse
