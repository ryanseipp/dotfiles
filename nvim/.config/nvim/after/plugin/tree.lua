local g = require'rs.globals'

require'nvim-tree'.setup {
    update_focused_file = {
        enable = true,
    },
    view = {
        width = 60,
        -- auto_resize = true,
    }
}

g.nnoremap('<leader>tt', '<cmd>NvimTreeToggle<CR>')
g.nnoremap('<leader>tr', '<cmd>NvimTreeRefresh<CR>')
