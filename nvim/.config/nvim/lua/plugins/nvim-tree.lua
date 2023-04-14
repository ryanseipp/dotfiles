return {
    'nvim-tree/nvim-tree.lua',
    keys = {
        { '<leader>tt', '<cmd>NvimTreeToggle<cr>', desc = 'NvimTreeToggle' },
        { '<leader>tf', '<cmd>NvimTreeFocus<cr>',  desc = 'NvimTreeFocus' }
    },
    dependencies = { 'kyazdani42/nvim-web-devicons' },
    opts = {
        update_focused_file = {
            enable = true,
        },
        view = {
            width = 60,
            -- auto_resize = true,
        }
    }
}
