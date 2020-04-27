
(.assembly hello_world (
    (.ver 0 0 0 0)
))

(.module "hello_world.jem")

(.class public HelloWorld (
    (.method public static void Main (((string []) args)) (
        .entrypoint
        (.maxstack 8)
        (ldstr "Hello World!")
        (call Console.WriteLine(string))
        ret
    ))
))