require('packer').startup({
    function(use)
        use 'wbthomason/packer.nvim'
        use {
            'neovim/nvim-lspconfig',
            requires = {
                'williamboman/mason.nvim',
                'williamboman/mason-lspconfig.nvim',
                'j-hui/fidget.nvim',
                'jose-elias-alvarez/null-ls.nvim'
            },
        }
        use {
            'hrsh7th/nvim-cmp',
            requires = {
                'hrsh7th/cmp-nvim-lsp',
                'hrsh7th/cmp-nvim-lua',
                'hrsh7th/cmp-buffer',
                'hrsh7th/cmp-path',
                'L3MON4D3/LuaSnip',
                'saadparwaiz1/cmp_luasnip',
                'onsails/lspkind-nvim',
            },
        }
        use { -- Telescope
            'nvim-telescope/telescope.nvim',
            branch = '0.1.x',
            requires = { 'nvim-lua/plenary.nvim' }
        }
        use { 'nvim-telescope/telescope-fzf-native.nvim', run = 'make' }
        use {
            'nvim-treesitter/nvim-treesitter',
            run = ':TSUpdate'
        }
        use {
            'nvim-treesitter/nvim-treesitter-textobjects',
            after = 'nvim-treesitter'
        }
        use 'windwp/nvim-ts-autotag'
        use 'windwp/nvim-autopairs'
        use 'numToStr/Comment.nvim'
        use { -- git integration
            'lewis6991/gitsigns.nvim',
            requires = {
                'nvim-lua/plenary.nvim'
            },
        }
        use { -- Startup page
            'goolord/alpha-nvim',
            requires = { 'kyazdani42/nvim-web-devicons' },
            config = function()
                require 'alpha'.setup(require 'alpha.themes.startify'.opts)
            end
        }
        use {
            'nvim-tree/nvim-tree.lua',
            requires = { 'kyazdani42/nvim-web-devicons' }
        }
        use 'stevearc/dressing.nvim' -- fancy selections
        use 'norcalli/nvim-colorizer.lua'
        use 'sainnhe/gruvbox-material'
        use 'navarasu/onedark.nvim'
        use 'lukas-reineke/indent-blankline.nvim'
        use {
            'nvim-lualine/lualine.nvim',
            requires = { 'kyazdani42/nvim-web-devicons', opt = true }
        }

        -- Code Completion
        -- use 'neovim/nvim-lspconfig' -- LSP client configurations
        -- use 'onsails/lspkind-nvim' -- LSP pictograms
        -- use 'L3MON4D3/LuaSnip' -- snippets
        -- use 'hrsh7th/nvim-cmp' -- Autocompletion plugin
        -- use 'hrsh7th/cmp-buffer' -- completion from buffer
        -- use 'hrsh7th/cmp-path' -- completion from path
        -- use 'hrsh7th/cmp-nvim-lua' -- nvim lua completion
        -- use 'hrsh7th/cmp-nvim-lsp' -- completion from lsp
        -- use 'saadparwaiz1/cmp_luasnip' -- snippets for cmp
        -- use 'jose-elias-alvarez/null-ls.nvim'

        -- Debugging
        -- use 'mfussenegger/nvim-dap'
        -- use 'rcarriga/nvim-dap-ui'
        -- use 'theHamsta/nvim-dap-virtual-text'

        -- use 'nvim-telescope/telescope-dap.nvim'
    end,
})
