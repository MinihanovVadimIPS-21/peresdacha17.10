using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseApp.Core.ViewModels;

namespace WarehouseApp.Tests;

[TestClass]
public class RelayCommandTests
{
    [TestMethod]
    public void Execute_InvokesAction()
    {
        var called = false;
        new RelayCommand(() => called = true).Execute(null);
        Assert.IsTrue(called);
    }

    [TestMethod]
    public void CanExecute_WithoutPredicate_ReturnsTrue()
    {
        Assert.IsTrue(new RelayCommand(() => { }).CanExecute(null));
    }

    [TestMethod]
    public void CanExecute_UsesPredicate()
    {
        var allowed = false;
        var cmd = new RelayCommand(() => { }, () => allowed);
        Assert.IsFalse(cmd.CanExecute(null));
        allowed = true;
        Assert.IsTrue(cmd.CanExecute(null));
    }

    [TestMethod]
    public void RaiseCanExecuteChanged_FiresEvent()
    {
        var fired = false;
        var cmd = new RelayCommand(() => { });
        cmd.CanExecuteChanged += (_, _) => fired = true;
        cmd.RaiseCanExecuteChanged();
        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void Constructor_NullAction_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new RelayCommand(null!));
    }
}
