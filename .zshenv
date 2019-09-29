# tmux
export TERM="xterm-256color"

# bspwm
export PANEL_FIFO="/tmp/panel-fifo"
export XDG_CONFIG_HOME="$HOME/.config"
export XDG_CACHE_HOME="$HOME/.local/share"

# oh-my-zsh
export ZSH=${XDG_CONFIG_HOME}/oh-my-zsh
export DEFAULT_USER="zorbik"

# powerlevel9k configuration
export POWERLEVEL9K_LEFT_PROMPT_ELEMENTS=(dir rbenv vcs)
export POWERLEVEL9K_RIGHT_PROMPT_ELEMENTS=(status root_indicator background_jobs history time)

# dotnet
export DOTNET_CLI_TELEMETRY_OPTOUT=1

# prison architect sound fix
export SND_AUDIOSYSTEM=pulse

# gpg
export GPG_TTY=$(tty)
export GNUPGHOME=${XDG_CONFIG_HOME}/gnupg

# conan
export CONAN_USER_HOME=$HOME/.local/share/conan

# docker
export DOCKER_CONFIG=${XDG_CONFIG_HOME}/docker

# nuget
export NUGET_PACKAGES=${XDG_CACHE_HOME}/NuGetPackages
# Add .NET Core SDK tools
export PATH="$PATH:/home/zorbik/.dotnet/tools"

# go
export GOPATH=$HOME/.local/share/go
