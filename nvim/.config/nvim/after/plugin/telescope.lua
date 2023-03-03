local telescope = require 'telescope'
telescope.setup {
    extensions = {
        fzf = {
            fuzzy = true,
            override_generic_sorter = true,
            override_file_sorter = true,
            case_mode = 'smart_case'
        },
    }
}

telescope.load_extension('fzf')

local g = require 'rs.globals'

local map_tele = function(key, f)
    local rhs = string.format("<cmd>lua require('rs.telescope')['%s']()<CR>", f)
    g.nnoremap(key, rhs, { silent = true })
end

-- dotfiles
map_tele('<leader>fn', 'find_nvim')
map_tele('<leader>fz', 'find_zsh')

-- files
map_tele('<leader>ff', 'find_files')
map_tele('<leader>fg', 'git_files')
map_tele('<leader>fp', 'project_search')
map_tele('<leader>fs', 'live_grep')

-- git
map_tele('<leader>gs', 'git_status')
map_tele('<leader>gc', 'git_commits')
map_tele('<leader>gb', 'git_branches')

-- lsp
map_tele('<leader>ltd', 'lsp_type_definitions')
map_tele('<leader>ld', 'lsp_definitions')
map_tele('<leader>li', 'lsp_implementations')
map_tele('<leader>lr', 'lsp_references')
map_tele('<leader>q', 'quickfix')

-- nvim
map_tele('<leader>nb', 'buffers')
map_tele('<leader>nj', 'jumplist')
map_tele('<leader>nr', 'registers')
map_tele('<leader>nc', 'commands')
map_tele('<leader>ncc', 'colorscheme')
map_tele('<leader>nh', 'help_tags')
map_tele('<leader>nk', 'keymaps')
map_tele('<leader>nac', 'autocommands')
