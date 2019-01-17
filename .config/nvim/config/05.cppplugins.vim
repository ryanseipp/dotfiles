" ======================== Ale ========================
" disable auto-complete
let g:ale_completion_enabled = 0

" only run linters named in ale_linters setting
let g:ale_linters_explicit = 1

"linter
let g:ale_linters = {
    \   'cpp': ['clang', 'gcc']
    \}

let cpp_options = '-std=c++17 -Wall -Wextra -Wshadow -Wnon-virtual-dtor
            \ -Wold-style-cast -Wcast-align -Wunused -Woverloaded-virtual
            \ -Wpedantic -Wconversion -Wsign-conversion -Wmisleading-indentation
            \ -Wduplicated-cond -Wduplicated-branches -Wlogical-op -Wnull-dereference
            \ -Wuseless-cast -Wdouble-promotion -Wformat=2 -Wlifetime'

let g:ale_cpp_clang_options = cpp_options
let g:ale_cpp_gcc_options = cpp_options

