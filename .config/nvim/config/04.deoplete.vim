" ================== Deoplete =====================
let g:deoplete#enable_at_startup = 1
let g:deoplete#enable_smart_case = 1

" disable autocomplete by default
let b:deoplete_disable_auto_complete = 1
let g:deoplete_disable_auto_complete = 1

let g:deoplete#sources = {}

call deoplete#custom#source('LanguageClient',
            \ 'min_pattern_length',
            \ 2)

" disable the candidates in comment/string syntaxes
call deoplete#custom#source('_',
            \ 'disabled_syntaxes', ['Comment', 'String'])

call deoplete#custom#option('sources', {
            \ 'cpp': ['LanguageClient'],
            \ 'c': ['LanguageClient'],
            \ 'python': ['LanguageClient'],
            \ 'python3': ['LanguageClient'],
            \ 'vim': ['vim'],
            \ })
