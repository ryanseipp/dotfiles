" (_)_ __ (_) \___   _(_)_ __ ___
" | | '_ \| | __\ \ / / | '_ `_  \
" | | | | | | |_ \ V /| | | | | | |
" |_|_| |_|_|\__(_)_/ |_|_| |_| |_|

" config files are split for maintainability
" in config/* (ex: 01.plugins.vim)

for f in split(glob('~/.config/nvim/config/*.vim'), '\n')
	exe 'source' f
endfor

