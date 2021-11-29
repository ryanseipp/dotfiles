require'nvim-treesitter.configs'.setup {
  ensure_installed = {"bash", "c", "c_sharp", "cmake", "comment", "cpp", "css", "dockerfile", "go", "haskell", "html", "java", "javascript", "json", "kotlin", "llvm", "lua", "nix", "regex", "rust", "scss", "toml", "tsx", "typescript", "vim", "yaml"},
  sync_install = false,
  highlight = {
    enable = true,
    additional_vim_regex_highlighting = false,
  },
  context_commentstring = {enable = true}
}
