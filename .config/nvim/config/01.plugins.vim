" ==================== VIM PLUG =======================
call plug#begin('~/.local/share/nvim/plugged')

" linting
Plug 'w0rp/ale'

" language client
Plug 'autozimu/LanguageClient-neovim', {
            \ 'branch': 'next',
            \ 'do': 'bash install.sh',
            \ }

Plug 'junegunn/fzf', { 'dir': '~/.fzf',
            \'do': './install --all' }
Plug 'junegunn/fzf.vim'

" syntax highlighting
Plug 'arakashic/chromatica.nvim'

" autocompletion
Plug 'Shougo/deoplete.nvim', { 'do': ':UpdateRemotePlugins' }

" switch between source and header files
Plug 'vim-scripts/a.vim'

" airline
Plug 'vim-airline/vim-airline'
Plug 'vim-airline/vim-airline-themes'

" color schemes
Plug 'chriskempson/base16-vim'

" automatic ({[<>]}) closing
Plug 'tpope/vim-surround'

Plug 'scrooloose/nerdtree', { 'on': 'NERDTreeToggle' }

call plug#end()

