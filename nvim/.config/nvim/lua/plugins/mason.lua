return {
    {
        'williamboman/mason.nvim',
        cmd = 'Mason',
        event = 'VeryLazy',
        config = true,
    },
    {
        'williamboman/mason-lspconfig.nvim',
        event = 'VeryLazy',
        opts = {
            ensure_installed = vim.tbl_keys(require('rs.lsp').servers)
        },
    },
}
