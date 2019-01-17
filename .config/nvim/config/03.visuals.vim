" ====================== Visuals ======================
syntax on

let base16colorspace=256
set t_Co=256

set background=dark
colorscheme base16-eighties

" =================== Number Column ===================
set number

" toggle relative numbering
autocmd InsertEnter * :set nornu
autocmd InsertLeave * :set rnu

" disable relative numbering while debugging
autocmd BufLeave * :set nornu
autocmd BufEnter * call SetRNU()
function! SetRNU()
    if(mode()!='i')
        set rnu
    endif
endfunction

" ====================== Airline ======================
let g:airline_theme='base16_eighties'

" vim airline fonts
if !exists('g:airline_symbols')
    let g:airline_symbols={}
endif

" tabline
let g:airline#extensions#tabline#enabled=1
let g:airline#extensions#tabline#formatter='unique_tail_improved'

" ale airline support
let g:airline#extensions#ale#enabled = 1

command! TablineON :set showtabline=1
command! TablineOFF :set showtabline=0

" unicode symbols
let g:airline_left_alt_sep = ''
let g:airline_right_alt_sep = ''
let g:airline_left_sep = ''
let g:airline_right_sep = ''
let g:airline_symbols.crypt = '🔒'
let g:airline_symbols.linenr = ''
let g:airline_symbols.maxlinenr = '☰'
let g:airline_symbols.branch = ''
let g:airline_symbols.readonly = ''
let g:airline_symbols.paste = 'ρ'
let g:airline_symbols.spell = 'Ꞩ'
let g:airline_symbols.notexists = '∄'
let g:airline_symbols.whitespace = 'Ξ'
let g:airline_powerline_fonts = 1
