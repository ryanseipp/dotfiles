local g = require'rs.globals'
local tree = require'nvim-tree'

tree.setup {
    update_focused_file = {
        enable = true,
    },
    view = {
        width = 60,
        auto_resize = true,
    }
}

g.map('n', '<leader>tt', '<cmd>NvimTreeToggle<CR>')
g.map('n', '<leader>tr', '<cmd>NvimTreeRefresh<CR>')
