return {
    'sainnhe/gruvbox-material',
    lazy = false,
    priority = 1000,
    dependencies = {
        { 'norcalli/nvim-colorizer.lua', config = true },
    },
    config = function()
        vim.cmd([[colorscheme gruvbox-material]])
    end,
    init = function()
        if vim.fn.has('termguicolors') then
            vim.o.termguicolors = true
        end

        vim.o.background = 'dark'
        vim.g.gruvbox_material_background = 'hard'
        vim.g.gruvbox_material_ui_contrast = 'high'
    end,
}
