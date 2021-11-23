require('packer').startup({
  function(use)
    use 'wbthomason/packer.nvim'          -- Package manager
    use 'neovim/nvim-lspconfig'           -- LSP client configurations
    use 'onsails/lspkind-nvim'            -- LSP pictograms
    use 'L3MON4D3/LuaSnip'                -- snippets
    use 'hrsh7th/nvim-cmp'                -- Autocompletion plugin
    use 'hrsh7th/cmp-buffer'              -- completion from buffer
    use 'hrsh7th/cmp-path'                -- completion from path
    use 'hrsh7th/cmp-nvim-lua'            -- nvim lua completion
    use 'hrsh7th/cmp-nvim-lsp'            -- completion from lsp
    use 'saadparwaiz1/cmp_luasnip'        -- snippets for cmp
    use 'nvim-treesitter/nvim-treesitter' -- Better syntax highlighting
    use 'eddyekofo94/gruvbox-flat.nvim'   -- color scheme
    use {'nvim-lualine/lualine.nvim', requires = {'kyazdani42/nvim-web-devicons', opt = true}}
end,
})

