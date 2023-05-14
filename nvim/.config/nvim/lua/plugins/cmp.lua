return {
    'hrsh7th/nvim-cmp',
    event = { "BufReadPre", "BufNewFile" },
    dependencies = {
        'hrsh7th/cmp-nvim-lsp',
        'hrsh7th/cmp-nvim-lua',
        'hrsh7th/cmp-buffer',
        'hrsh7th/cmp-path',
        'L3MON4D3/LuaSnip',
        'saadparwaiz1/cmp_luasnip',
    },
    opts = function()
        local cmp = require('cmp')
        return {
            mapping = {
                ["<C-n>"] = cmp.mapping.select_next_item { behavior = cmp.SelectBehavior.Insert },
                ["<C-p>"] = cmp.mapping.select_prev_item { behavior = cmp.SelectBehavior.Insert },
                ['<C-d>'] = cmp.mapping.scroll_docs(-4),
                ['<C-f>'] = cmp.mapping.scroll_docs(4),
                ['<C-e>'] = cmp.mapping.close(),
                ['<c-y>'] = cmp.mapping.confirm {
                    behavior = cmp.ConfirmBehavior.Insert,
                    select = true,
                },
                ['<c-Space>'] = cmp.mapping.complete(),
            },
            sources = {
                { name = 'nvim_lua' },
                { name = 'nvim_lsp' },
                { name = 'path' },
                { name = 'luasnip' },
                { name = 'buffer',  keyword_length = 5 },
            },
            snippet = {
                expand = function(args)
                    require('luasnip').lsp_expand(args.body)
                end,
            },
            experimental = {
                native_menu = false,
            },
        }
    end,
}
