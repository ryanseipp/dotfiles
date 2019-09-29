" ================ Language Client ================
let g:LanguageClient_serverCommands = {
            \ 'cpp': ['cquery'],
            \ 'c': ['cquery'],
            \ 'javascript': ['tcp://127.0.0.1:2089'],
            \ 'javascript.jsx': ['tcp://127.0.0.1:2089'],
            \ 'typescript': ['tcp://127.0.0.1:2089'],
            \ 'typescript.tsx': ['/usr/local/bin/javascript-typescript-stdio'],
            \ }

let g:LanguageClient_autoStart = 1

let g:LanguageClient_rootMarkers = {
            \ 'cpp': ['compile_commands.json', 'build'],
            \ 'c': ['compile_commands.json', 'build'],
            \ 'javascript': ['package.json'],
            \ }

set completefunc=LanguageClient#complete
set formatexpr=LanguageClient_textDocument_rangeFormatting()

let g:LanguageClient_loadSettings = 1
let g:LanguageClient_settingsPath = '~/.config/nvim/settings.json'

" keybinds
function SetLSPShortcuts()
    nnoremap <silent> gh :call LanguageClient#textDocument_hover()<cr>
    nnoremap <silent> gd :call LanguageClient#textDocument_definition()<cr>
    nnoremap <silent> gr :call LanguageClient#textDocument_references()<cr>
    nnoremap <silent> gs :call LanguageClient#textDocument_documentSymbol<cr>
    nnoremap <F2> :call LanguageClient_textDocument_rename()<cr>
endfunction()

augroup LSP
    autocmd!
    autocmd FileType cpp,c call SetLSPShortcuts()
augroup END

