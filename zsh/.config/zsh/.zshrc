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

autoload -U +X bashcompinit && bashcompinit
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

source "$ZDOTDIR/zsh-functions"

zsh_add_file "zsh-exports"
zsh_add_file "zsh-aliases"

zsh_add_plugin "zsh-users/zsh-autosuggestions"
zsh_add_plugin "zsh-users/zsh-syntax-highlighting"

[ -z "$NVM_DIR" ] && export NVM_DIR="$HOME/.local/share/nvm"
source /usr/share/nvm/nvm.sh --no-use

source /usr/share/nvm/bash_completion
source /usr/share/nvm/install-nvm-exec

export mmove() {
    while :; do
        if  [ $(xprintidle) -gt 100000 ]; then
            x=$(rand -M 5120)
            y=$(rand -M 1440)
            xdotool mousemove $x $y;
        fi

        sleep 30
    done
}

export h() {
    "$@" --help 2>&1 | bat -p -l help
}

eval "$(starship init zsh)"
