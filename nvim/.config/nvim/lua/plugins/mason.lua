return {
    'williamboman/mason.nvim',
    cmd = 'Mason',
    event = { "VeryLazy" },
    config = true,
    dependencies = {
        { 'williamboman/mason-lspconfig.nvim', opts = { ensure_installed = vim.tbl_keys(require('rs.lsp').servers) } }
    },
}
