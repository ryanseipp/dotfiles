#!/bin/sh

setopt autocd
setopt extendedglob
setopt nomatch
setopt automenu
setopt interactive_comments
setopt extended_history
setopt hist_expire_dups_first
setopt hist_ignore_dups
setopt hist_ignore_space
setopt hist_verify
setopt share_history

eval "$(/opt/homebrew/bin/brew shellenv)"
source "$ZDOTDIR/zsh-functions"

zsh_add_file "zsh-exports"
zsh_add_file "zsh-aliases"
zsh_add_file ".zshenv"

FPATH="$XDG_CONFIG_HOME/zsh/completions/:$(brew --prefix)/share/zsh/site-functions:$FPATH"

autoload -U +X bashcompinit && bashcompinit
source $(brew --prefix)/etc/bash_completion.d/az
autoload -U +X compinit && compinit -d $XDG_CACHE_HOME/zsh/zcompdump-$ZSH_VERSION

stty stop undef
zle_highlight=('paste:none')

unsetopt BEEP

zstyle ':completion:*' menu select cache-path $XDG_CACHE_HOME/zsh/zcompcache
zmodload zsh/complist
_comp_options+=(globdots)

autoload -U up-line-or-beginning-search
autoload -U down-line-or-beginning-search
zle -N up-line-or-beginning-search
zle -N down-line-or-beginning-search
bindkey "^[[A" up-line-or-beginning-search
bindkey "^[[B" down-line-or-beginning-search

autoload -Uz colors && colors

zsh_add_plugin "zsh-users/zsh-autosuggestions"
zsh_add_plugin "zsh-users/zsh-syntax-highlighting"

eval "$(fnm env --use-on-cd)"
eval "$(starship init zsh)"
