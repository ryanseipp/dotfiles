return {
    'nvim-telescope/telescope.nvim',
    branch = '0.1.x',
    cmd = "Telescope",
    keys = {
        -- dotfiles
        {
            '<leader>fn',
            "<cmd>lua require('rs.telescope')['find_nvim']()<cr>",
            desc = 'find_nvim',
            silent = true
        },
        { '<leader>fz',  "<cmd>lua require('rs.telescope')['find_zsh']()<cr>",     desc = 'find_zsh',     silent = true },

        -- files
        {
            '<leader>ff',
            "<cmd>lua require('rs.telescope')['find_files']()<cr>",
            desc = 'find_files',
            silent = true
        },
        {
            '<leader>fg',
            "<cmd>lua require('rs.telescope')['git_files']()<cr>",
            desc = 'git_files',
            silent = true
        },
        {
            '<leader>fp',
            "<cmd>lua require('rs.telescope')['project_search']()<cr>",
            desc = 'project_search',
            silent = true
        },
        {
            '<leader>fs',
            "<cmd>lua require('rs.telescope')['live_grep']()<cr>",
            desc = 'live_grep',
            silent = true
        },

        -- git
        {
            '<leader>gs',
            "<cmd>lua require('rs.telescope')['git_status']()<cr>",
            desc = 'git_status',
            silent = true
        },
        {
            '<leader>gc',
            "<cmd>lua require('rs.telescope')['git_commits']()<cr>",
            desc = 'git_commits',
            silent = true
        },
        {
            '<leader>gb',
            "<cmd>lua require('rs.telescope')['git_branches']()<cr>",
            desc = 'git_branches',
            silent = true
        },

        -- lsp
        {
            '<leader>ltd',
            "<cmd>lua require('rs.telescope')['lsp_type_definitions']()<cr>",
            desc = 'lsp_type_definitions',
            silent = true
        },
        {
            '<leader>ld',
            "<cmd>lua require('rs.telescope')['lsp_definitions']()<cr>",
            desc = 'lsp_definitions',
            silent = true
        },
        {
            '<leader>li',
            "<cmd>lua require('rs.telescope')['lsp_implementations']()<cr>",
            desc = 'lsp_implementations',
            silent = true
        },
        {
            '<leader>lr',
            "<cmd>lua require('rs.telescope')['lsp_references']()<cr>",
            desc = 'lsp_references',
            silent = true
        },
        {
            '<leader>q',
            "<cmd>lua require('rs.telescope')['quickfix']()<cr>",
            desc = 'quickfix',
            silent = true
        },

        -- nvim
        { '<leader>nb',  "<cmd>lua require('rs.telescope')['buffers']()<cr>",      desc = 'buffers',      silent = true },
        { '<leader>nj',  "<cmd>lua require('rs.telescope')['jumplist']()<cr>",     desc = 'jumplist',     silent = true },
        { '<leader>nr',  "<cmd>lua require('rs.telescope')['registers']()<cr>",    desc = 'registers',    silent = true },
        { '<leader>nc',  "<cmd>lua require('rs.telescope')['commands']()<cr>",     desc = 'commands',     silent = true },
        { '<leader>ncc', "<cmd>lua require('rs.telescope')['colorscheme']()<cr>",  desc = 'colorscheme',  silent = true },
        { '<leader>nh',  "<cmd>lua require('rs.telescope')['help_tags']()<cr>",    desc = 'help_tags',    silent = true },
        { '<leader>nk',  "<cmd>lua require('rs.telescope')['keymaps']()<cr>",      desc = 'keymaps',      silent = true },
        { '<leader>nac', "<cmd>lua require('rs.telescope')['autocommands']()<cr>", desc = 'autocommands', silent = true },
    },
    dependencies = {
        'nvim-lua/plenary.nvim',
        'nvim-telescope/telescope-dap.nvim',
        { 'nvim-telescope/telescope-fzf-native.nvim', build = 'make' },
    },
    opts = {
        extensions = {
            fzf = {
                fuzzy = true,
                override_generic_sorter = true,
                override_file_sorter = true,
                case_mode = 'smart_case'
            },
        },
    },
    config = function(telescope)
        telescope.load_extension('fzf')
    end
}
