" ==================== Suggestions ====================
" show menu of suggestions when typing commands in command mode
set path+=**
set wildmenu
set showcmd

" =================  File Management ==================
" turn off swap files
set noswapfile
set nobackup
set nowb

" reload files changed outside vim
set autoread

" trigger autoread when files change on disk
autocmd FocusGained,BufEnter,CursorHold,CursorHoldI *
 \ if mode() != 'c' | checktime | endif

" notification after file change
autocmd FileChangedShellPost *
 \ echohl WarningMsg | echo "File changed on disk. Buffer reloaded." | echohl None

" ======================= Folds =======================
set foldmethod=indent
set foldnestmax=3
set nofoldenable

" ===================== Scrolling =====================
" start scrolling when 8 lines away from margins
set scrolloff=8

" ===================== Encoding ======================
" set encoding to utf8
if &encoding != 'utf-8'
	set encoding=utf-8
endif

" ==================== Indentation ====================
set autoindent
set smartindent
set shiftwidth=4
set tabstop=4
set smarttab
set expandtab

" soft tab stop, makes spaces feel like tabs when deleting
set sts=4

" ===================== Searching =====================
set ignorecase
set incsearch
set smartcase
set hlsearch
nnoremap <F3> :set hlsearch!<CR>

" ==================== Performance ====================
" fix slow scrolling when using mouse and rel numbers
set lazyredraw

" ================ Keyboard Bindings ==================

" set leader key to comma
let mapleader = ','

" copy to clipboard
noremap <C-c> "+y

" paste from clipboard
noremap <C-v> "+p

" cut to clipboard
noremap <C-x> "+d

" paste in insert mode
noremap <C-v> <Esc>"+pa

" fast scrolling
nnoremap J 10j
vnoremap J 10j

nnoremap K 10k
vnoremap K 10k

" stay in normal mode after inserting new line
noremap o o <Bs><Esc>
noremap O O <Bs><Esc>

" map U to redo
noremap U <c-r>
noremap <c-r> <NOP>

" ======================= Misc ========================
" highlight matching braces
set showmatch

" disable blinking of matching braces
set mat=0

" always show status line
set laststatus=2

set nowrap
set history=1000

" fix backspace for systems where it is broken
set backspace=indent,eol,start

" disable preview window
set completeopt-=preview

" always draw signcolumn
set signcolumn=yes

" autoclose if only window left is nerdtree
autocmd BufEnter * if (winnr("$") == 1 && exists("b:NERDTree") && b:NERDTree.isTabTree()) | q | endif

