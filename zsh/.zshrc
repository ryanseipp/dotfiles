export ZDOTDIR=$HOME/.config/zsh
source "$ZDOTDIR/.zshrc"

# opam configuration
[[ ! -r /home/zorbik/.opam/opam-init/init.zsh ]] || source /home/zorbik/.opam/opam-init/init.zsh  > /dev/null 2> /dev/null
