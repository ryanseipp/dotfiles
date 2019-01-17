# Lines configured by zsh-newuser-install
HISTFILE=~/.histfile
HISTSIZE=1000
SAVEHIST=1000
setopt appendhistory autocd notify
unsetopt beep
bindkey -e
# End of lines configured by zsh-newuser-install
# The following lines were added by compinstall
zstyle :compinstall filename '/home/ryan/.zshrc'

autoload -Uz compinit
compinit
# End of lines added by compinstall

case $(tty) in
  (/dev/tty[1-9]) ZSH_THEME="dieter";;
              (*) ZSH_THEME="powerlevel9k/powerlevel9k";;
esac

if [ -r $HOME/.aliases.zsh ]; then
  source $HOME/.aliases.zsh
fi

if [ -r $HOME/.zshenv ]; then
  source $HOME/.zshenv
fi

source $ZSH/oh-my-zsh.sh

DISABLE_AUTO_UPDATE="true"

plugins=(
  git
  git-prompt
  zsh-autosuggestions
)

[ -f ~/.fzf.zsh ] && source ~/.fzf.zsh

