vim.cmd [[packadd packer.nvim]]

require('packer').startup({
  function(use)
    use 'wbthomason/packer.nvim'          -- Package manager
    use 'lewis6991/impatient.nvim'        -- speed up require

    -- startup
    use {
        'goolord/alpha-nvim',
        requires = {'kyazdani42/nvim-web-devicons'},
        config = function()
            require'alpha'.setup(require'alpha.themes.startify'.opts)
        end
    }

    -- Tree Sitter
    use {
        'nvim-treesitter/nvim-treesitter',
        run = ':TSUpdate'
    }
    use 'windwp/nvim-ts-autotag'
    use 'windwp/nvim-autopairs'

    -- Code Completion
    use 'neovim/nvim-lspconfig'           -- LSP client configurations
    use 'onsails/lspkind-nvim'            -- LSP pictograms
    use 'L3MON4D3/LuaSnip'                -- snippets
    use 'hrsh7th/nvim-cmp'                -- Autocompletion plugin
    use 'hrsh7th/cmp-buffer'              -- completion from buffer
    use 'hrsh7th/cmp-path'                -- completion from path
    use 'hrsh7th/cmp-nvim-lua'            -- nvim lua completion
    use 'hrsh7th/cmp-nvim-lsp'            -- completion from lsp
    use 'saadparwaiz1/cmp_luasnip'        -- snippets for cmp

    -- Debugging
    use 'mfussenegger/nvim-dap'
    use 'rcarriga/nvim-dap-ui'
    use 'theHamsta/nvim-dap-virtual-text'

    -- Telescope
    use {
        'nvim-telescope/telescope.nvim',
        branch = '0.1.x',
        requires = {{'nvim-lua/plenary.nvim'}}
    }
    use {'nvim-telescope/telescope-fzf-native.nvim', run = 'make'}
    use 'nvim-telescope/telescope-hop.nvim'
    use 'nvim-telescope/telescope-dap.nvim'

    -- git integration
    use {
        'lewis6991/gitsigns.nvim',
        requires = {
            'nvim-lua/plenary.nvim'
        },
    }

    -- File tree
    use {
        'kyazdani42/nvim-tree.lua',
        requires = {'kyazdani42/nvim-web-devicons'}
    }

    -- fancy selections
    use 'stevearc/dressing.nvim'

    -- useful keybinds
    use { 'numToStr/Comment.nvim' }
    use {'phaazon/hop.nvim', branch = 'v1' }

    -- Colors
    use 'norcalli/nvim-colorizer.lua'
    use 'sainnhe/gruvbox-material'

    -- Status line
    use {
        'nvim-lualine/lualine.nvim',
        requires = {'kyazdani42/nvim-web-devicons', opt = true}
    }
end,
})

vim.cmd [[
    augroup packer_user_config
        autocmd!
        autocmd BufWritePost plugins.lua source <afile> | PackerCompile
    augroup end
]]

